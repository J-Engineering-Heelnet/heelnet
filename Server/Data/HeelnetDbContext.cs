using Microsoft.EntityFrameworkCore;

namespace Heelnet.Data {
    public class HeelnetDbContext(DbContextOptions<HeelnetDbContext> options) : DbContext(options) {

    }
}
