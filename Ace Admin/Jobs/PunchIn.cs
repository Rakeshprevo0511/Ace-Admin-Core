using Ace_Admin.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Ace_Admin.Jobs
{
    public class PunchIn: IJob
    {
        private readonly PracticeDbContext _dbContext;

        public PunchIn(PracticeDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var count = await _dbContext.Employees.CountAsync();
            Console.WriteLine($"Total Employees: {count}");

            await Task.CompletedTask;
        }
    }
}
