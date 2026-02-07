using AbcYazilim.OgrenciTakip.Data.Contexts;
using System.Data.Entity.Migrations;

namespace AbcYazilim.OgrenciTakip.Data.OgrenciTakipMigration
{
    public class Configuration:DbMigrationsConfiguration<OgrenciTakipContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            // Entity Framework'ün model değişikliklerini otomatik olarak algılayıp
            // migration oluşturmadan veritabanına uygulamasını sağlar

            AutomaticMigrationDataLossAllowed = true;
            // Otomatik migration sırasında veri kaybına neden olabilecek işlemlere
            // (kolon silme, tablo silme vb.) izin verir. Production ortamında dikkatli kullanılmalıdır.

        }
    }
}
