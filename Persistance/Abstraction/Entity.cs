using FFXIVVenues.Veni.Persistance.Abstraction;
using System;

namespace LinqEm.Persistence.Abstraction
{
    public abstract class Entity : IEntity
    {

        public abstract string Id { get; protected set; }

        public Entity()
        {
            this.Id = GenerateId();
        }

        public string GenerateId()
        {
            var chars = "BCDFGHJKLMNPQRSTVWXYZbcdfghjklmnpqrstvwxyz0123456789";
            var stringChars = new char[12];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }

    }
}
