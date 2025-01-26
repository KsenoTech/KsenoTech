using minesweeperAPI.ApplicationCore.DomModels;
using minesweeperAPI.ApplicationCore.Interfaces.Repositories;
using minesweeperAPI.Infrastructure.BLL.Services;

namespace minesweeperAPI.Infrastructure.DAL.Repositories
{
    public class DbRepository : IDbRepository
    {
        private MinesweeperContext _dbcontext;

        private GameRepositorySQL _gameRepository;

        public DbRepository(MinesweeperContext dbcontext)
        {
            _dbcontext = dbcontext;
        }



        public IRepository<Game> Games
        {
            get
            {
                if (_gameRepository == null)
                    _gameRepository = new GameRepositorySQL(_dbcontext);
                return _gameRepository;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _dbcontext.SaveChangesAsync();
        }
    }
}
