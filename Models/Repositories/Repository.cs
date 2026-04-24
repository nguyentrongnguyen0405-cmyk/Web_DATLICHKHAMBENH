using Microsoft.EntityFrameworkCore;
using Web_Đặt_lịch_phòng_khám.Data; // Đảm bảo đúng namespace DbContext của bạn
using Web_Đặt_lịch_phòng_khám.Repositories;

namespace Web_Đặt_lịch_phòng_khám.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // Lấy tất cả danh sách
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        // Lấy theo ID
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        // Thêm mới
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        // Cập nhật (Sửa lỗi CS0535 - thiếu phương thức UpdateAsync)
        public async Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        // Xóa (Sửa lỗi CS0535 - thiếu phương thức DeleteAsync)
        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        // Lưu thay đổi vào Database
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}