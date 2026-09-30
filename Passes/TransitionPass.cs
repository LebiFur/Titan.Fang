using System;
using System.Collections.Generic;
using System.Linq;

namespace Titan.Fang
{
    public sealed class TransitionPass : Pass
    {
        public override QueueAbilities RequiredAbilities => QueueAbilities.None;
        public override IEnumerable<Image> ReadImages => [];
        public override IEnumerable<Image> WrittenImages => transitions.Select(x => x.Image);
        public override ImageLayout ReadLayout => ImageLayout.None;
        public override ImageLayout WriteLayout => ImageLayout.None;
        public override ImageUsage ReadUsage => ImageUsage.None;
        public override ImageUsage WriteUsage => ImageUsage.None;

        public IReadOnlyList<ImageTransition> Transitions => transitions;

        private readonly ImageTransition[] transitions;

        public TransitionPass(string name, IEnumerable<ImageTransition> transitions) : base(name)
        {
            if (!transitions.TryGetNonEnumeratedCount(out int count)) count = transitions.Count();

            if (count == 0) throw new Exception("No image transitions");

            this.transitions = new ImageTransition[count];

            int i = 0;
            foreach (ImageTransition transition in transitions)
            {
                this.transitions[i] = transition;
                i++;
            }
        }
    }
}
