
using Entities.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Interfaces;
using Persistence.Repository.GenericRepository;

namespace Persistence.Repository;

public class PersonRepository : GenericRepository<PersonEntity>, IPersonRepository
{
    public PersonRepository(IdentityDbContext apiContext)
        : base(apiContext)
    {

    }


    public async Task<List<PersonEntity>> GetPeopleAsync(List<int> ids)
    {
        return (await Context.Set<PersonEntity>()
           .Where(p => ids.Any(id => id == p.Id)).ToListAsync());
    }
}
