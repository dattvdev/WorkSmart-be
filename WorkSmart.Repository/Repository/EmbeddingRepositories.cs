using Microsoft.EntityFrameworkCore;
using WorkSmart.Core.Entity;
using WorkSmart.Repository;

public interface ICVEmbeddingRepository
{
    Task<CVEmbedding?> GetByCVId(int cvId);
    Task SaveOrUpdate(int cvId, string vectorJson);
    Task Delete(int cvId);
}

public interface IJobEmbeddingRepository
{
    Task<IEnumerable<JobEmbedding>> GetAll();
    Task<JobEmbedding?> GetByJobId(int jobId);
    Task SaveOrUpdate(int jobId, string vectorJson);
    Task Delete(int jobId);
}

public class CVEmbeddingRepository : ICVEmbeddingRepository
{
    private readonly WorksmartDBContext _context;
    public CVEmbeddingRepository(WorksmartDBContext context) => _context = context;

    public async Task<CVEmbedding?> GetByCVId(int cvId) => await _context.CVEmbeddings.FindAsync(cvId);

    public async Task SaveOrUpdate(int cvId, string vectorJson)
    {
        var entity = await _context.CVEmbeddings.FindAsync(cvId);
        if (entity == null)
        {
            entity = new CVEmbedding { CVID = cvId, VectorJson = vectorJson };
            _context.CVEmbeddings.Add(entity);
        }
        else
        {
            entity.VectorJson = vectorJson;
            entity.UpdatedAt = DateTime.Now;
        }
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int cvId)
    {
        var entity = await _context.CVEmbeddings.FindAsync(cvId);
        if (entity != null)
        {
            _context.CVEmbeddings.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}

public class JobEmbeddingRepository : IJobEmbeddingRepository
{
    private readonly WorksmartDBContext _context;
    public JobEmbeddingRepository(WorksmartDBContext context) => _context = context;

    public async Task<JobEmbedding?> GetByJobId(int jobId) => await _context.JobEmbeddings.FindAsync(jobId);

    public async Task SaveOrUpdate(int jobId, string vectorJson)
    {
        var entity = await _context.JobEmbeddings.FindAsync(jobId);
        if (entity == null)
        {
            entity = new JobEmbedding { JobID = jobId, VectorJson = vectorJson };
            _context.JobEmbeddings.Add(entity);
        }
        else
        {
            entity.VectorJson = vectorJson;
            entity.UpdatedAt = DateTime.Now;
        }
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int jobId)
    {
        var entity = await _context.JobEmbeddings.FindAsync(jobId);
        if (entity != null)
        {
            _context.JobEmbeddings.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<JobEmbedding>> GetAll()
    {
        return await _context.JobEmbeddings.ToListAsync();
    }
}