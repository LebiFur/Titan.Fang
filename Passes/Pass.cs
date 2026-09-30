using System.Collections.Generic;

namespace Titan.Fang
{
    public abstract class Pass
    {
        public Queue? Queue { get; internal set; }

        public string Name { get; }

        public abstract QueueAbilities RequiredAbilities { get; }
        public abstract IEnumerable<Image> ReadImages { get; }
        public abstract IEnumerable<Image> WrittenImages { get; }
        public abstract ImageLayout ReadLayout { get; }
        public abstract ImageLayout WriteLayout { get; }
        public abstract ImageUsage ReadUsage { get; }
        public abstract ImageUsage WriteUsage { get; }

        protected Pass(string name)
        {
            Name = name;
        }
    }
}
