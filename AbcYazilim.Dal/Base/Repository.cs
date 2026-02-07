using AbcYazilim.Dal.Interfaces;
using AbcYazilim.OgrenciTakip.Common.Enums;
using AbcYazilim.OgrenciTakip.Common.Functions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace AbcYazilim.Dal.Base
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private bool disposedValue;
        private readonly DbContext _context;
        private readonly DbSet<T> _dbSet;
        /// <summary>
        /// <see cref="Repository{T}"/> sınıfının yeni bir örneğini başlatır.
        /// Veritabanı bağlamını (DbContext) ve ilgili varlık kümesini (DbSet) yapılandırır.
        /// </summary>
        /// <param name="context">Veritabanı işlemlerini yürütecek olan <see cref="DbContext"/> nesnesi.</param>
        /// <remarks>
        /// Eğer gönderilen context null ise, atama işlemleri yapılmaz ve nesne yapılandırılmadan bırakılır.
        /// </remarks>
        public Repository(DbContext context)
        {
            if (context == null) return;
            _context = context;
            _dbSet = _context.Set<T>();
        }
        /// <summary>
        /// Belirtilen varlığı (entity) veritabanına eklenmek üzere işaretler.
        /// </summary>
        /// <param name="entity">Veritabanına eklenecek olan <typeparamref name="T"/> tipindeki varlık.</param>
        /// <remarks>
        /// Bu metot varlığın durumunu doğrudan <see cref="EntityState.Added"/> olarak ayarlar. 
        /// Değişikliklerin veritabanına yansıması için SaveChanges metodu çağrılmalıdır.
        /// </remarks>
        public void Insert(T entity)
        {
            _context.Entry(entity).State = EntityState.Added;
        }
        /// <summary>
        /// Belirtilen varlık koleksiyonundaki tüm öğeleri veritabanına eklenmek üzere işaretler.
        /// </summary>
        /// <param name="entities">Veritabanına eklenecek olan <typeparamref name="T"/> tipindeki varlıkların listesi.</param>
        /// <remarks>
        /// Döngü kullanarak her bir öğenin durumunu <see cref="EntityState.Added"/> olarak ayarlar.
        /// Büyük veri setlerinde performans için bu işlemden sonra SaveChanges metodu bir kez çağrılmalıdır.
        /// </remarks>
        public void Insert(IEnumerable<T> entities)
        {
            foreach (var item in entities)
                _context.Entry(item).State = EntityState.Added;
        }
        /// <summary>
        /// Mevcut bir varlığı (entity) veritabanında güncellenmek üzere işaretler.
        /// </summary>
        /// <param name="entity">Güncellenecek verileri içeren <typeparamref name="T"/> tipindeki varlık.</param>
        /// <remarks>
        /// Nesnenin durumunu <see cref="EntityState.Modified"/> olarak işaretleyerek, 
        /// SaveChanges çağrıldığında ilgili satırın SQL UPDATE komutu ile güncellenmesini sağlar.
        /// </remarks>
        public void Update(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }
        /// <summary>
        /// Bir varlığın sadece belirtilen özelliklerini (alanlarını) güncellenmek üzere işaretler.
        /// </summary>
        /// <param name="entity">Güncellenecek verileri içeren varlık.</param>
        /// <param name="fields">Güncellenmesi istenen özellik isimlerinin (property names) listesi.</param>
        /// <remarks>
        /// Varlığı önce bağlama (context) ekler (Attach), ardından sadece <paramref name="fields"/> 
        /// parametresinde belirtilen alanların <see cref="PropertyEntry.IsModified"/> durumunu true yapar.
        /// Bu sayede SQL tarafında sadece ilgili kolonlar için UPDATE sorgusu oluşturulur.
        /// </remarks>
        public void Update(T entity, IEnumerable<string> fields)
        {
            _dbSet.Attach(entity);
            var entry = _context.Entry(entity);
            foreach (var field in fields)
                entry.Property(field).IsModified = true;
        }
        /// <summary>
        /// Belirtilen varlık koleksiyonundaki tüm öğeleri veritabanında güncellenmek üzere işaretler.
        /// </summary>
        /// <param name="entities">Güncellenecek verileri içeren <typeparamref name="T"/> tipindeki varlıkların listesi.</param>
        /// <remarks>
        /// Her bir öğe için durumu <see cref="EntityState.Modified"/> olarak ayarlar.
        /// İşlemin veritabanına yansıması için döngüden sonra bir kez SaveChanges çağrılması yeterlidir.
        /// </remarks>
        public void Update(IEnumerable<T> entities)
        {
            foreach (var item in entities)
                _context.Entry(item).State = EntityState.Modified;
        }
        /// <summary>
        /// Belirtilen varlığı (entity) veritabanından silinmek üzere işaretler.
        /// </summary>
        /// <param name="entity">Veritabanından silinecek olan <typeparamref name="T"/> tipindeki varlık.</param>
        /// <remarks>
        /// Varlığın durumunu <see cref="EntityState.Deleted"/> olarak işaretler. 
        /// Değişiklikler SaveChanges metodu çağrıldığında fiziksel silme (Hard Delete) olarak veritabanına yansıtılır.
        /// </remarks>
        public void Delete(T entity)
        {
            _context.Entry(entity).State = EntityState.Deleted;
        }
        /// <summary>
        /// Belirtilen varlık koleksiyonundaki tüm öğeleri veritabanından silinmek üzere işaretler.
        /// </summary>
        /// <param name="entities">Veritabanından silinecek olan <typeparamref name="T"/> tipindeki varlıkların listesi.</param>
        /// <remarks>
        /// Her bir öğe için durumu <see cref="EntityState.Deleted"/> olarak ayarlar.
        /// İşlem tamamlandığında SaveChanges çağrısı ile tüm öğeler veritabanından fiziksel olarak kaldırılır.
        /// </remarks>
        public void Delete(IEnumerable<T> entities)
        {
            foreach (var item in entities)
                _context.Entry(item).State = EntityState.Deleted;
        }

        /// <summary>
        /// Belirtilen filtreye uygun ilk kaydı seçer ve istenen veri tipine dönüştürerek döndürür.
        /// </summary>
        /// <typeparam name="TResult">Dönüştürülecek (Project edilecek) hedef veri tipi.</typeparam>
        /// <param name="filter">Kaydı bulmak için kullanılacak filtre ifadesi (Lambda expression). Null geçilirse filtrelenmeden ilk kayıt alınır.</param>
        /// <param name="selector">Varlığın hangi özelliklerinin seçileceğini belirten seçim ifadesi.</param>
        /// <returns>Kayıt bulunursa <typeparamref name="TResult"/> tipinde dönüştürülmüş veri, bulunamazsa null döner.</returns>
        /// <remarks>
        /// Bu metot "Projection" yaparak veritabanından sadece ihtiyaç duyulan kolonların çekilmesini sağlar, 
        /// bu da performansı optimize eder ve gereksiz veri transferini önler.
        /// </remarks>
        public TResult Find<TResult>(Expression<Func<T, bool>> filter, Expression<Func<T, TResult>> selector)
        {
            return filter == null ? _dbSet.Select(selector).FirstOrDefault() : _dbSet.Where(filter).Select(selector).FirstOrDefault();
        }


        /// <summary>
        /// Belirtilen filtreye uygun tüm kayıtları seçer ve istenen veri tipine dönüştürerek bir sorgu nesnesi (<see cref="IQueryable{T}"/>) olarak döndürür.
        /// </summary>
        /// <typeparam name="TResult">Dönüştürülecek (Project edilecek) hedef veri tipi.</typeparam>
        /// <param name="filter">Kayıtları filtrelemek için kullanılacak kriter. Null ise tüm kayıtlar seçilir.</param>
        /// <param name="selector">Her bir varlığın hangi özelliklerinin seçileceğini belirten ifade.</param>
        /// <returns>Veritabanı seviyesinde henüz çalıştırılmamış, dönüştürülmüş sonuç sorgusu.</returns>
        /// <remarks>
        /// <see cref="IQueryable"/> dönüş tipi sayesinde, veritabanına asıl sorgu gönderilmeden önce üzerine ek filtrelemeler veya sıralamalar eklenebilir. 
        /// Bu, büyük veri setlerinde sayfalama (paging) işlemleri için idealdir.
        /// </remarks>
        public IQueryable<TResult> Select<TResult>(Expression<Func<T, bool>> filter, Expression<Func<T, TResult>> selector)
        {
            return filter == null ? _dbSet.Select(selector) : _dbSet.Where(filter).Select(selector);
        }
       

        /// <summary>
        /// Veritabanındaki kayıt sayısını, verilen filtreye göre veya filtre olmaksızın döndürür.
        /// </summary>
        /// <param name="filter">Kayıtları süzmek için kullanılacak olan LINQ ifadesi (Opsiyonel).</param>
        /// <returns>Kayıt kümesindeki toplam eleman sayısı.</returns>
        /// <remarks>
        /// Metot, filtre verilmediği durumda tablodaki tüm kayıtları, filtre verildiğinde ise 
        /// sadece kritere uyan kayıtları sayar. SQL tarafında verinin tamamını çekmek yerine 
        /// doğrudan <c>COUNT</c> sorgusu çalıştırarak bellek ve işlemci tasarrufu sağlar.
        /// </remarks
        public int Count(Expression<Func<T, bool>> filter = null)
        {
            return filter==null? _dbSet.Count() : _dbSet.Count(filter);
        }
        /// <summary>
        /// Belirtilen kart türüne ve kriterlere göre bir sonraki benzersiz artışlı kodu oluşturur.
        /// </summary>
        /// <param name="kartTuru">Kodun ön eki için temel alınacak kart türü (enum).</param>
        /// <param name="filter">Kodun okunacağı alanı belirleyen ifade (Genellikle Kod alanı).</param>
        /// <param name="where">Kod aranırken uygulanacak ek filtreleme kriteri. Null ise tüm tablo taranır.</param>
        /// <returns>Yeni oluşturulan artışlı kod stringi (Örn: "OKUL-0002").</returns>
        /// <remarks>
        /// Metot, veritabanında kayıt yoksa "0001" ile başlayan bir başlangıç kodu üretir. 
        /// Eğer mevcut kayıt varsa, en büyük değeri bulur ve sonundaki sayısal değeri bir artırarak formatı korur.
        /// </remarks>
        /// 
        public string YeniKodVer(KartTuru kartTuru, Expression<Func<T, string>> filter, Expression<Func<T, bool>> where = null)
        {
            string Kod()
            {
                string kod = null;
                var kodDizi = kartTuru.ToName().Split(' ');
                for (int i = 0; i < kodDizi.Length - 1; i++)
                {
                    kod += kodDizi[i];

                    if (i + 1 < kodDizi.Length - 1)
                        kod += " ";
                }
                return kod += "-0001";
            }
            string YeniKodVer(string kod) 
            {
                var sayisalDegerler = "";
                foreach(var karakter in kod)
                {
                    if (char.IsDigit(karakter))
                        sayisalDegerler += karakter;
                    else
                        sayisalDegerler = ""; 
                }
                var artisSonrasiDeger=(int.Parse(sayisalDegerler)+1).ToString();
                var fark=kod.Length-artisSonrasiDeger.Length;
                if(fark==0)
                    fark = 0;
                var yeniDeger=kod.Substring(0,fark);
                yeniDeger += artisSonrasiDeger;
                
                return yeniDeger;
            }
            var maxKod=where==null?_dbSet.Max(filter):_dbSet.Where(where).Max(filter);
            return maxKod==null?Kod():YeniKodVer(maxKod);
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _context.Dispose();
                }

                disposedValue = true;
            }
        }



        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        #endregion
    }
}
