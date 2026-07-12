using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using EnglishLearningPlatformApp.Authors;
using EnglishLearningPlatformApp.Books;
using EnglishLearningPlatformApp.Content;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace EnglishLearningPlatformApp.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class EnglishLearningPlatformAppDbContext :
    AbpDbContext<EnglishLearningPlatformAppDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    public DbSet<Author> Authors { get; set; }

    public DbSet<Book> Books { get; set; }
    public DbSet<ContentItem> ContentItems { get; set; }
    public DbSet<ContentVersion> ContentVersions { get; set; }

    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext and ISaasDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext and ISaasDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public EnglishLearningPlatformAppDbContext(DbContextOptions<EnglishLearningPlatformAppDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();

        builder.Entity<Author>(b =>
        {
            b.ToTable(EnglishLearningPlatformAppConsts.DbTablePrefix + "Authors",
                EnglishLearningPlatformAppConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.Name).IsRequired().HasMaxLength(AuthorConsts.MaxNameLength);
            b.Property(x => x.ShortBio).HasMaxLength(AuthorConsts.MaxShortBioLength);
        });

        builder.Entity<Book>(b =>
        {
            b.ToTable(EnglishLearningPlatformAppConsts.DbTablePrefix + "Books",
                EnglishLearningPlatformAppConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.HasOne<Author>().WithMany().HasForeignKey(x => x.AuthorId).IsRequired();
        });

        builder.Entity<ContentItem>(b =>
        {
            b.ToTable(EnglishLearningPlatformAppConsts.DbTablePrefix + "ContentItems", EnglishLearningPlatformAppConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasMany(x => x.Versions).WithOne().HasForeignKey(x => x.ContentItemId).OnDelete(DeleteBehavior.Cascade);
            b.Navigation(x => x.Versions).UsePropertyAccessMode(PropertyAccessMode.Field);
            b.HasIndex(x => new { x.TenantId, x.Status });
        });

        builder.Entity<ContentVersion>(b =>
        {
            b.ToTable(EnglishLearningPlatformAppConsts.DbTablePrefix + "ContentVersions", EnglishLearningPlatformAppConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Title).IsRequired().HasMaxLength(ContentConsts.MaxTitleLength);
            b.HasIndex(x => new { x.ContentItemId, x.VersionNumber }).IsUnique();
            b.HasIndex(x => x.ContentItemId).IsUnique().HasFilter("[Lifecycle] = 0");
            b.HasIndex(x => new { x.TenantId, x.Lifecycle });
        });

        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(EnglishLearningPlatformAppConsts.DbTablePrefix + "YourEntities", EnglishLearningPlatformAppConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
    }
}
