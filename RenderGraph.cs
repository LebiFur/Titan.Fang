using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Titan.Fang
{
    public sealed class RenderGraph
    {
        public bool IsBaked { get; private set; }

        public string Name { get; }

        public IReadOnlyList<PlacedImage> PlacedImages => placedImages;
        public IReadOnlyList<ExternalImage> ExternalImages => externalImages;
        public IReadOnlyList<Pass> Passes => passes;
        public IReadOnlyList<Queue> Queues => queues;
        public IReadOnlyList<RenderPassAssembly>? RenderPassAssemblies => renderPassAssemblies;
        public IReadOnlyList<PlacedImageBucket>? PlacedImageBuckets => placedImageBuckets;

        private readonly Action<string>? log;
        
        private List<PlacedImage> placedImages = [];
        private List<ExternalImage> externalImages = [];
        private List<Pass> passes = [];
        private List<Queue> queues = [];
        private List<RenderPassAssembly>? renderPassAssemblies;
        private List<PlacedImageBucket>? placedImageBuckets;

        public RenderGraph(string name, Action<string>? log = null)
        {
            this.log = log;

            Name = name;
        }

        public Queue CreateQueue(string name, QueueAbilities abilities)
        {
            if (IsBaked) throw new Exception("RenderGraph is already baked");

            if ((abilities & QueueAbilities.CrossQueueTransition) != 0 &&
                queues.Any(x => x.HasAbilities(QueueAbilities.CrossQueueTransition)))
                throw new Exception("Can't create more than 1 Queue with QueueAbilities.CrossQueueTransition");

            Queue queue = new(name, abilities);

            queues.Add(queue);

            return queue;
        }

        public ExternalImage CreateExternalImage(
            string name,
            ExternalImageHandle handle,
            string? handleId,
            ImageFormat format,
            ImageSizing sizing,
            Vector2D<uint>? constantSize = null)
        {
            if (IsBaked) throw new Exception("RenderGraph is already baked");

            ExternalImage image = new(name, handle, handleId, format, sizing, constantSize);

            externalImages.Add(image);

            return image;
        }

        public PlacedImage CreatePlacedImage(
            string name,
            PlacedImageHandle handle,
            string? handleId,
            ImageFormat format,
            ImageSizing sizing,
            Vector2D<uint>? constantSize = null,
            bool neverCull = false,
            bool neverAlias = false)
        {
            if (IsBaked) throw new Exception("RenderGraph is already baked");

            PlacedImage image = new(name, handle, handleId, format, sizing, constantSize, neverCull, neverAlias);

            placedImages.Add(image);

            return image;
        }

        public void AddPass(Queue queue, Pass pass)
        {
            if (IsBaked) throw new Exception("RenderGraph is already baked");

            if (!queue.HasAbilities(pass.RequiredAbilities)) throw new Exception($"Queue \"{queue.Name}\" doesn't have required abilities");

            pass.Queue = queue;

            passes.Add(pass);
        }

        public void InsertPass(Queue queue, Pass pass, int index)
        {
            if (IsBaked) throw new Exception("RenderGraph is already baked");

            if (!queue.HasAbilities(pass.RequiredAbilities)) throw new Exception($"Queue \"{queue.Name}\" doesn't have required abilities");

            pass.Queue = queue;

            passes.Insert(index, pass);

            foreach (PlacedImage image in placedImages)
            {
                if (!image.Lifetime.HasValue) return;

                int startPassIndex = image.Lifetime.Value.StartPassIndex;
                int endPassIndex = image.Lifetime.Value.EndPassIndex;

                if (index <= startPassIndex) startPassIndex++;
                if (index <= endPassIndex) endPassIndex++;

                image.Lifetime = new(startPassIndex, endPassIndex);
            }
        }

        public void Bake()
        {
            if (IsBaked) return;

            int placedImagesCount;
            int externalImagesCount;
            int passesCount;

            do
            {
                placedImagesCount = placedImages.Count;
                externalImagesCount = externalImages.Count;
                passesCount = passes.Count;

                CullPlacedImages();
                CullExternalImages();
                CullPasses();
            } while (placedImagesCount != placedImages.Count || externalImagesCount != externalImages.Count || passesCount != passes.Count);

            ValidatePasses();

            CreateAndAssignRenderPassAssemblies();

            CullQueues();

            SetPlacedImagesLifetimes();
            SetPlacedImagesUsages();

            CreateAndAssignPlacedImageBuckets();

            CreateTransitionPasses();

            IsBaked = true;
        }

        private void CullPlacedImages()
        {
            List<PlacedImage> culledPlacedImages = new(placedImages.Count);

            foreach (PlacedImage image in placedImages)
            {
                if (image.NeverCull)
                {
                    culledPlacedImages.Add(image);

                    continue;
                }

                bool read = false;
                bool written = false;

                foreach (Pass pass in passes)
                {
                    if (pass.ReadImages.Contains(image)) read = true;
                    if (pass.WrittenImages.Contains(image)) written = true;

                    if (read && written) break;
                }

                bool culled = false;

                if (!read && !written) culled = true;
                else if (read && !written) throw new Exception($"PlacedImage \"{image.Name}\" is read but is never written");
                else if (!read && written) culled = true;

                if (culled) log?.Invoke($"PlacedImage \"{image.Name}\" has been culled");
                else culledPlacedImages.Add(image);
            }

            placedImages = culledPlacedImages;
        }

        private void CullExternalImages()
        {
            List<ExternalImage> culledExternalImages = new(externalImages.Count);

            foreach (ExternalImage image in externalImages)
            {
                bool readOrWritten = false;

                foreach (Pass pass in passes)
                {
                    if (pass.ReadImages.Contains(image)) readOrWritten = true;
                    else if (pass.WrittenImages.Contains(image)) readOrWritten = true;

                    if (readOrWritten) break;
                }

                if (!readOrWritten) log?.Invoke($"ExternalImage \"{image.Name}\" has been culled");
                else culledExternalImages.Add(image);
            }

            externalImages = culledExternalImages;
        }

        private void CullPasses()
        {
            List<Pass> culledPasses = new(passes.Count);

            foreach (Pass pass in passes)
            {
                bool culled = false;

                if (!pass.WrittenImages.Any()) culled = true;
                else
                {
                    IEnumerable<Image> culledImages = pass.WrittenImages
                        .Where(x => placedImages.Any(y => y == x) || externalImages.Any(y => y == x));

                    if (!culledImages.Any()) culled = true;
                    else
                    {
                        IEnumerable<string> faultyImages = pass.WrittenImages
                            .Where(x => !culledImages.Contains(x)).Select(x => x.Name);

                        if (faultyImages.Any())
                            throw new Exception($"Pass \"{pass.Name}\" writes to images that no longer exists: {string.Join(", ", faultyImages)}");
                    }
                }
                
                if (culled) log?.Invoke($"Pass \"{pass.Name}\" has been culled");
                else culledPasses.Add(pass);
            }

            passes = culledPasses;
        }

        private void ValidatePasses()
        {
            foreach (Pass pass in passes)
            {
                foreach (Image image in pass.ReadImages)
                {
                    if (pass.WrittenImages.Contains(image))
                        throw new Exception($"Image \"{image.Name}\" is read and written in the same pass \"{pass.Name}\"");
                }
            }
        }

        private void CreateAndAssignRenderPassAssemblies()
        {
            renderPassAssemblies = [];

            foreach (Pass pass in passes)
            {
                if (pass is not GraphicsPass graphicsPass) continue;

                RenderPassAssembly assembly = new(renderPassAssemblies.Count, graphicsPass);

                foreach (RenderPassAssembly otherAssembly in renderPassAssemblies)
                {
                    if (assembly == otherAssembly)
                    {
                        graphicsPass.Assembly = otherAssembly;
                        break;
                    }
                }

                if (graphicsPass.Assembly is null)
                {
                    graphicsPass.Assembly = assembly;

                    renderPassAssemblies.Add(assembly);
                }
            }
        }

        private void CullQueues()
        {
            List<Queue> culledQueues = new(queues.Count);

            foreach (Queue queue in queues)
            {
                if (passes.Any(x => x.Queue == queue)) culledQueues.Add(queue);
                else log?.Invoke($"Queue \"{queue.Name}\" has been culled");
            }

            queues = culledQueues;
        }

        private void CreateTransitionPasses()
        {
            static void Transition(
                Dictionary<Image, ImageLayout> lastLayouts,
                List<ImageTransition> transitions,
                Image image,
                ImageLayout newLayout)
            {
                if (lastLayouts.TryGetValue(image, out ImageLayout oldLayout))
                {
                    if (oldLayout == newLayout) return;
                }
                else oldLayout = ImageLayout.Undefined;

                transitions.Add(new(
                    image,
                    oldLayout,
                    newLayout,
                    image.GetLastMemoryLayout()
                ));

                image.SetLastMemoryLayout(newLayout);

                lastLayouts[image] = newLayout;
            }

            Dictionary<Image, ImageLayout> lastLayouts = [];

            List<(Queue queue, TransitionPass pass, int index)> passesToAdd = [];

            int transitionIndex = 0;

            for (int i = 0; i < passes.Count; i++)
            {
                Pass pass = passes[i];

                List<ImageTransition> transitions = [];

                if (pass.ReadLayout != ImageLayout.None)
                    foreach (Image image in pass.ReadImages) Transition(lastLayouts, transitions, image, pass.ReadLayout);

                if (pass.WriteLayout != ImageLayout.None)
                    foreach (Image image in pass.WrittenImages) Transition(lastLayouts, transitions, image, pass.WriteLayout);

                if (transitions.Count > 0)
                {
                    passesToAdd.Add((pass.Queue!, new($"Auto transition {transitionIndex}", transitions), i));

                    transitionIndex++;
                }
            }

            for (int i = 0; i < passesToAdd.Count; i++)
            {
                (Queue queue, TransitionPass pass, int index) pass = passesToAdd[i];

                InsertPass(pass.queue, pass.pass, pass.index + i);
            }
        }

        private void SetPlacedImagesLifetimes()
        {
            foreach (PlacedImage image in placedImages)
            {
                int start;
                int end;

                if (image.NeverAlias)
                {
                    start = 0;
                    end = passes.Count - 1;
                }
                else
                {
                    start = -1;
                    end = -1;

                    for (int i = 0; i < passes.Count; i++)
                    {
                        Pass pass = passes[i];

                        if (pass.ReadImages.Contains(image) || pass.WrittenImages.Contains(image))
                        {
                            if (start == -1) start = i;
                            end = i;
                        }
                    }
                }

                image.Lifetime = new(start, end);
            }
        }

        private void SetPlacedImagesUsages()
        {
            foreach (PlacedImage image in placedImages)
            {
                ImageUsage usage = ImageUsage.None;

                foreach (Pass pass in passes)
                {
                    if (pass.ReadImages.Contains(image)) usage |= pass.ReadUsage;
                    if (pass.WrittenImages.Contains(image)) usage |= pass.WriteUsage;
                }

                image.Usage = usage;
            }
        }

        private void CreateAndAssignPlacedImageBuckets()
        {
            placedImageBuckets = [];

            foreach (PlacedImage image in placedImages)
            {
                if (image.Bucket != null) continue;

                List<PlacedImage> bucketImages = [];

                foreach (PlacedImage otherImage in placedImages)
                {
                    if (image == otherImage) continue;

                    if (otherImage.Bucket != null) continue;

                    if (!ImageLifetime.Intersects(image.Lifetime!.Value, otherImage.Lifetime!.Value))
                        bucketImages.Add(otherImage);
                }

                bool bucketChanged;

                do
                {
                    bucketChanged = false;

                    foreach (PlacedImage bucketImage in bucketImages)
                    {
                        foreach (PlacedImage otherBucketImage in bucketImages)
                        {
                            if (bucketImage == otherBucketImage) continue;

                            if (ImageLifetime.Intersects(bucketImage.Lifetime!.Value, otherBucketImage.Lifetime!.Value))
                            {
                                bucketImages.Remove(otherBucketImage);

                                bucketChanged = true;
                                break;
                            }
                        }

                        if (bucketChanged) break;
                    }
                }
                while (bucketChanged);

                bucketImages.Add(image);

                PlacedImageBucket bucket = new(placedImageBuckets.Count, bucketImages);

                foreach (PlacedImage bucketImage in bucketImages)
                {
                    bucketImage.Bucket = bucket;
                }

                placedImageBuckets.Add(bucket);
            }
        }
    }
}
