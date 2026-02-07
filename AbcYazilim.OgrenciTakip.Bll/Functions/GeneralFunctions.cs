using AbcYazilim.Dal.Base;
using AbcYazilim.Dal.Interfaces;
using AbcYazilim.OgrenciTakip.Model.Entities.Base.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;

namespace AbcYazilim.OgrenciTakip.Bll.Functions
{
    public static class GeneralFunctions
    {
        /// </summary>
        /// <typeparam name="T">Karşılaştırılacak nesnelerin tipi.</typeparam>
        /// <param name="oldEntity">Nesnenin orijinal/eski hali.</param>
        /// <param name="currentEntity">Nesnenin güncel/yeni hali.</param>
        /// <returns>Değişiklik saptanan özellik isimlerini (property names) içeren bir liste döndürür.</returns>
        /// <remarks>
        /// Metot şu özel durumları yönetir:
        /// <list type="bullet">
        /// <item><description>Jenerik koleksiyonları (Navigation Properties) karşılaştırma dışı bırakır.</description></item>
        /// <item><description>Resim gibi <see cref="byte"/> dizilerini uzunluk kontrolü yaparak karşılaştırır.</description></item>
        /// <item><description>Null değerleri karşılaştırma sırasında boş string olarak ele alarak hataları önler.</description></item>
        /// </list>
        /// </remarks>
        public static IList<string> DegisenAlanlariGetir<T>(this T oldEntity,T currentEntity) 
        {
            IList<string> alanlar=new List<string>();

            foreach (var prop in currentEntity.GetType().GetProperties())
            {
                if (prop.PropertyType.Namespace == "System.Collections.Generic") continue;
                var oldValue = prop.GetValue(oldEntity)??string.Empty;
                var currentValue=prop.GetValue(currentEntity)??string.Empty;

                if (prop.PropertyType == typeof(byte[]))
                {
                    if (string.IsNullOrEmpty(oldValue.ToString()))
                        oldValue = new byte[] {0};
                    if(string.IsNullOrEmpty(currentValue.ToString()))
                        currentValue = new byte[] {0};
                    if (((byte[])oldValue).Length != ((byte[])currentValue).Length)
                        alanlar.Add(prop.Name);
                }
                else if(!currentValue.Equals(oldValue))
                    alanlar.Add(prop.Name);
            }
            return alanlar;
        }
        /// <summary>
        /// Yapılandırma dosyasından (App.config/Web.config) projeye ait veritabanı bağlantı dizesini okur.
        /// </summary>
        /// <returns>
        /// "OgrenciTakipContext" anahtarı ile eşleşen bağlantı dizesini (ConnectionString) string türünde döndürür.
        /// </returns>
        /// <remarks>
        /// Bu metot, veritabanı sunucu adresi, veritabanı adı ve kimlik doğrulama bilgilerini kod içerisine gömmeden 
        /// merkezi bir noktadan yönetilmesini sağlar. Yapılandırma dosyasında bu isimle bir anahtar bulunamazsa hata fırlatabilir.
        /// </remarks>
        public static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["OgrenciTakipContext"].ConnectionString;
        }
        /// <summary>
        /// Belirtilen <see cref="DbContext"/> tipinde yeni bir veritabanı bağlamı örneği oluşturur.
        /// </summary>
        /// <typeparam name="TContext">Oluşturulacak olan ve <see cref="DbContext"/> sınıfından türetilmiş bağlam tipi.</typeparam>
        /// <returns>
        /// Veritabanı bağlantı dizesi enjekte edilmiş, kullanıma hazır bir <typeparamref name="TContext"/> örneği döndürür.
        /// </returns>
        /// <remarks>
        /// Metot, <see cref="Activator.CreateInstance(Type, object[])"/> yöntemini kullanarak 
        /// ilgili DbContext'in bağlantı dizesi alan constructor'ını tetikler.
        /// </remarks>
        private static TContext CreateContext<TContext>() where TContext:DbContext
        {
            return (TContext)Activator.CreateInstance(typeof(TContext), GetConnectionString());
        }
        /// <summary>
        /// Verilen referans üzerinden yeni bir <see cref="IUnitOfWork{T}"/> örneği oluşturur.
        /// </summary>
        /// <typeparam name="T">İşlem yapılacak olan ve <see cref="IBaseEntity"/> arayüzünü uygulayan varlık tipi.</typeparam>
        /// <typeparam name="TContext">Veritabanı bağlamı (<see cref="DbContext"/>) tipi.</typeparam>
        /// <param name="uow">Oluşturulacak veya yenilenecek olan <see cref="IUnitOfWork{T}"/> referansı.</param>
        /// <remarks>
        /// Metot çalıştırıldığında, eğer <paramref name="uow"/> parametresi null değilse mevcut nesne <see cref="IDisposable.Dispose"/> edilerek bellekten temizlenir. 
        /// Ardından <see cref="CreateContext{TContext}"/> metodu kullanılarak taze bir bağlantı ile yeni bir iş birimi örneği başlatılır.
        /// </remarks>
        public static void CreateUnitOfWork<T,TContext>(ref IUnitOfWork<T> uow) where T:class,IBaseEntity where TContext:DbContext
        {
            uow?.Dispose();
            uow = new UnitOfWork<T>(CreateContext<TContext>());
        }
        
    }
}
