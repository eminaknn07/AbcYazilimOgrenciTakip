using AbcYazilim.Dal.Interfaces;
using AbcYazilim.OgrenciTakip.Common.Message;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace AbcYazilim.Dal.Base
{
    public class UnitOfWork<T> : IUnitOfWork<T> where T : class
    {
        private bool _disposedValue;
        private readonly DbContext _context;

        /// <summary>
        /// <see cref="UnitOfWork"/> sınıfının yeni bir örneğini başlatır.
        /// </summary>
        /// <param name="context">
        /// Repository'ler arasında paylaşılacak ve işlemlerin merkezi olarak yönetilmesini sağlayacak olan <see cref="DbContext"/> nesnesi.
        /// </param>
        /// <remarks>
        /// Gönderilen context nesnesi null ise atama yapılmaz. Bu sınıf, veritabanı işlemlerinin atomik (ya hep ya hiç) 
        /// bir şekilde yürütülmesinden sorumludur.
        /// </remarks>
        public UnitOfWork(DbContext context)
        {
            if (context == null) return;
            _context = context;
        }
        public IRepository<T> Rep => new Repository<T>(_context);

        /// <summary>
        /// Yapılan tüm değişiklikleri veritabanına kaydeder ve oluşabilecek hataları yönetir.
        /// </summary>
        /// <returns>İşlem başarılı ise true, herhangi bir hata oluşursa false döner.</returns>
        /// <remarks>
        /// Metot, veritabanı kısıtlamaları (Foreign Key, Unique Index vb.) ve bağlantı hatalarını 
        /// SQL hata kodları üzerinden analiz ederek kullanıcıya <see cref="Messages"/> sınıfı aracılığıyla anlamlı geri bildirimler verir.
        /// <list type="bullet">
        /// <item><description><b>547:</b> İlişkili kayıt hatası (Silme engeli).</description></item>
        /// <item><description><b>2601/2627:</b> Mükerrer kayıt/ID hatası.</description></item>
        /// <item><description><b>18456:</b> Yetkilendirme hatası.</description></item>
        /// </list>
        /// </remarks>
        public bool Save()
        {
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                var sqlExp = (SqlException)ex.InnerException?.InnerException;
                if (sqlExp == null)
                {
                    Messages.HataMesaji(ex.Message);
                    return false;
                }
                switch (sqlExp.Number)
                {
                    case 208:
                        Messages.HataMesaji("İşlem Yapmak istediğiniz Tablo Veritabanında Bulunamadı.");
                        break;
                    case 547:
                        Messages.HataMesaji("Seçilen kartın işlem Görmüş hareketleri Var Kart Silinemez.");
                        break;
                    case 2627:
                    case 2601:
                        Messages.HataMesaji("Girmiş Olduğunuz ID Daha Önce Kullanılmıştır.");
                        break;
                    case 4060:
                        Messages.HataMesaji("İşlem Yapmak İstediğiniz Veritabanı Sunucuda Bulanamadı.");
                        break;
                    case 18456:
                        Messages.HataMesaji("Sunucuya Bağlanılmak istenilen Kullanıcı Adı veya Şifre Hatalıdır.");
                        break;
                    default:
                        Messages.HataMesaji(sqlExp.Message);
                        break;
                }
                return false;
            }
            catch (Exception ex)
            {
                Messages.HataMesaji(ex.Message); return false;

            }
            return true;
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposedValue = true;
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
