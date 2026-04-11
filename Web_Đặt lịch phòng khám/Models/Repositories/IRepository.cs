namespace Web_Đặt_lịch_phòng_khám.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity); // Thêm dòng này để sửa lỗi 'UpdateAsync'
        Task DeleteAsync(int id);
        Task SaveAsync();
    }
}