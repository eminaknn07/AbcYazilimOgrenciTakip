using AbcYazilim.OgrenciTakip.Data.OgrenciTakipMigration;
using AbcYazilim.OgrenciTakip.Model.Entities;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace AbcYazilim.OgrenciTakip.Data.Contexts
{
    /// <summary>
    /// Uygulamanýn ana veritabaný baðlamýdýr (Database Context).
    /// </summary>
    /// <remarks>
    /// <see cref="BaseDbContext{TContext, TConfiguration}"/> sýnýfýndan türetilerek, 
    /// veritabaný baðlantý yönetimi ve otomatik Migration (göç) yeteneklerini devralýr. 
    /// Bu sýnýf, Entity Framework üzerinden veritabaný tablolarý ile C# sýnýflarý arasýndaki iletiþimi saðlar.
    /// </remarks>
    public class OgrenciTakipContext : BaseDbContext<OgrenciTakipContext,Configuration>
    {
        /// <summary>
        /// Varsayýlan baðlantý dizesini kullanarak baðlamý baþlatýr ve Lazy Loading'i devre dýþý býrakýr.
        /// </summary>
        public OgrenciTakipContext()
        {
            Configuration.LazyLoadingEnabled=false;
            // Navigation property'ler üzerinden iliþkili tablolarýn
            // otomatik olarak çekilmesini engeller (performans ve kontrol için)
        }
        /// <summary>
        /// Belirtilen baðlantý dizesini kullanarak baðlamý baþlatýr ve Lazy Loading'i devre dýþý býrakýr.
        /// </summary>
        /// <param name="connectionString">Veri tabaný baðlantý dizesi.</param>
        public OgrenciTakipContext(string connectionString) : base(connectionString)
        {
            Configuration.LazyLoadingEnabled=false;
            // Navigation property'ler üzerinden iliþkili tablolarýn
            // otomatik olarak çekilmesini engeller (performans ve kontrol için)
        }
        /// <summary>
        /// Veri tabaný modeli oluþturulurken varsayýlan kurallarý (Conventions) özelleþtirir.
        /// </summary>
        /// <param name="modelBuilder">Model oluþturma kurallarýný yöneten bileþen.</param>
        /// <remarks>
        /// Bu metotta þu deðiþiklikler yapýlmýþtýr:
        /// <list type="bullet">
        /// <item><description>Tablo isimlerinin Ýngilizce çoðullaþtýrma kurallarýna göre (s takýsý) oluþturulmasý engellenmiþtir.</description></item>
        /// <item><description>Bire-çok ve çoða-çok iliþkilerde, bir ana kayýt silindiðinde baðlý kayýtlarýn otomatik silinmesi (Cascade Delete) güvenlik nedeniyle kapatýlmýþtýr.</description></item>
        /// </list>
        /// </remarks>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            // Tablo isimlerinin otomatik çoðullaþtýrýlmasýný engeller

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            // One-to-many iliþkilerde varsayýlan cascade delete (zincirleme silme) davranýþýný kapatýr.
            // Böylece ana kayýt silindiðinde baðlý kayýtlar otomatik olarak silinmez.

            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            // Many-to-many iliþkilerde varsayýlan cascade delete (zincirleme silme) davranýþýný kapatýr.
            // Böylece bir kayýt silindiðinde, iliþkili kayýtlar otomatik olarak silinmez.
        }

        public DbSet<Il> Il { get; set; }
        public DbSet<Ilce> Ilce{ get; set; }
        public DbSet<Okul> Okul { get; set; }
    }
}