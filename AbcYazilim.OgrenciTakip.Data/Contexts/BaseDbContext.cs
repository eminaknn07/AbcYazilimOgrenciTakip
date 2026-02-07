using System.Data.Entity;
using System.Data.Entity.Migrations;

namespace AbcYazilim.OgrenciTakip.Data.Contexts
{
    /// <summary>
    /// Uygulama genelinde kullanılacak olan veritabanı bağlamları (DbContext) için temel yapılandırma sınıfıdır.
    /// </summary>
    /// <typeparam name="TContext">Migration işlemlerinin uygulanacağı hedef DbContext tipi.</typeparam>
    /// <typeparam name="TConfiguration">
    /// <see cref="DbMigrationsConfiguration{TContext}"/> sınıfından türetilmiş, veritabanı şema versiyonlarını yöneten yapılandırma tipi.
    /// </typeparam>
    /// <remarks>
    /// Bu sınıf, veritabanı başlatma stratejilerini (Initializer) merkezi bir noktada toplar. 
    /// Statik yapıcı metot (Constructor) içerisinde veritabanını en güncel Migration seviyesine otomatik olarak yükseltir.
    /// </remarks>
    public class BaseDbContext<TContext,TConfiguration>:DbContext where TContext:DbContext where TConfiguration:DbMigrationsConfiguration<TContext>,new()
    {
        private static string _nameOrConnectionString=typeof(TContext).Name;
        /// <summary>
        /// Veritabanı bağlamını (DbContext) varsayılan bağlantı dizesi adı ile başlatır.
        /// </summary>
        /// <remarks>
        /// Bağlantı adı olarak, jenerik <typeparamref name="TContext"/> tipinin sınıf adı kullanılır.
        /// </remarks>
        public BaseDbContext():base(_nameOrConnectionString){ }

        /// <summary>
        /// Veritabanı bağlamını belirtilen bağlantı dizesi ile başlatır ve veritabanını en güncel sürüme yükseltir.
        /// </summary>
        /// <param name="connectionString">Kullanılacak olan veritabanı bağlantı dizesi veya App.config içindeki bağlantı adı.</param>
        /// <remarks>
        /// Bu kurucu metot çalıştırıldığında <see cref="MigrateDatabaseToLatestVersion{TContext, TConfiguration}"/> 
        /// başlatıcısı devreye girer. Bu sayede uygulama her ayağa kalktığında veritabanı modelindeki 
        /// değişiklikler (Migration) otomatik olarak SQL tarafına uygulanır.
        /// </remarks>
        public BaseDbContext(string connectionString):base(connectionString)
        {
            // Uygulama başlarken veritabanını mevcut model ile senkronize eder ve
            // eksik migration'ları otomatik olarak uygular
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<TContext, TConfiguration>());
            _nameOrConnectionString = connectionString;
        }
        
    }
}
