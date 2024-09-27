using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using ModelTables;

public class DbContextModel : IdentityDbContext<IdentityUser>
{
    public DbSet<User> User { get; set; }
    public DbSet<Message> Message { get; set; }
    public DbSet<Room> Room { get; set; }

    public static bool _created = false;
    public DbContextModel(DbContextOptions<DbContextModel> options)
    : base(options)
    {
        if (!_created)
        {
            _created = true;
            Database.EnsureCreated();
        }
    }

    /* user - message:
       um user para muitas mensagens

       user - room:
       um user para muitas salas

       room - message:
       muitos rooms para muitas mensagens
    */
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // user:
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);
        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .ValueGeneratedOnAdd();
        modelBuilder.Entity<User>()
            .HasMany(a => a.Rooms)
            .WithOne(r => r.Adm)
            .HasForeignKey(r => r.AdmId);
        modelBuilder.Entity<User>()
            .HasMany(u => u.Messages)
            .WithOne(m => m.User)
            .HasForeignKey(m => m.UserId);

        // message:
        modelBuilder.Entity<Message>()
            .HasKey(m => m.Id);
        modelBuilder.Entity<Message>()
            .Property(m => m.Id)
            .ValueGeneratedOnAdd();

        // room:
        modelBuilder.Entity<Room>()
            .HasKey(r => r.Id);
        modelBuilder.Entity<Room>()
            .HasOne(r => r.Adm)
            .WithMany(a => a.Rooms)
            .HasForeignKey(r => r.AdmId);
        modelBuilder.Entity<Message>()
            .HasOne(m => m.User)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.UserId);
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Room)
            .WithMany(r => r.Messages)
            .HasForeignKey(m => m.RoomId);

        base.OnModelCreating(modelBuilder);
    }
}
