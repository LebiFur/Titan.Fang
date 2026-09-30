namespace Titan.Fang
{
    public sealed class Queue
    {
        public string Name { get; }
        public QueueAbilities Abilities { get; }

        internal Queue(string name, QueueAbilities abilities)
        {
            Name = name;
            Abilities = abilities;
        }

        public bool HasAbilities(QueueAbilities abilities) => (Abilities & abilities) == abilities;
    }
}
