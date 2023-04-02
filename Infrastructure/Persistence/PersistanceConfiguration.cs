namespace FFXIVVenues.Veni.Infrastructure.Persistence
{
    internal class PersistenceConfiguration
    {

        public PersistanceProvider Provider { get; set; }

		public string ConnectionString { get; set; }

    }

    internal enum PersistanceProvider
    {
        InMemory,
        LiteDb,
        Cosmos
    }
}
