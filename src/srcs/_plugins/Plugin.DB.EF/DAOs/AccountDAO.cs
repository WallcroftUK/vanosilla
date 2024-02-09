// WingsEmu
// 
// Developed by NosWings Team

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PhoenixLib.DAL;
using PhoenixLib.Logging;
using Plugin.Database.DB;
using Plugin.Database.Entities.Account;
using WingsAPI.Data.Account;

namespace Plugin.Database.DAOs
{
    public class AccountDAO : IAccountDAO
    {
        private readonly IDbContextFactory<GameContext> _contextFactory;
        private readonly IMapper<AccountEntity, AccountDTO> _mapper;
        private readonly IGenericAsyncLongRepository<AccountDTO> _repository;

        public AccountDAO(IMapper<AccountEntity, AccountDTO> mapper, IDbContextFactory<GameContext> contextFactory, IGenericAsyncLongRepository<AccountDTO> repository)
        {
            _mapper = mapper;
            _contextFactory = contextFactory;
            _repository = repository;
        }

        public AccountDTO LoadByName(string name)
        {
            try
            {
                using GameContext context = _contextFactory.CreateDbContext();
                AccountEntity accountEntity = context.Account.FirstOrDefault(a => a.Name.Equals(name));
                return accountEntity == null ? null : _mapper.Map(accountEntity);
            }
            catch (Exception e)
            {
                Log.Error("LoadByName", e);
                return null;
            }
        }

        public async Task<AccountDTO> GetByNameAsync(string name)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                AccountEntity accountEntity = await context.Account.FirstOrDefaultAsync(a => a.Name == name);
                return accountEntity == null ? null : _mapper.Map(accountEntity);
            }
            catch (Exception e)
            {
                Log.Error("LoadByName", e);
                return null;
            }
        }


        public async Task<List<AccountDTO>> LoadByMasterAccountIdAsync(Guid masterAccountId)
        {
            try
            {
                await using GameContext context = _contextFactory.CreateDbContext();
                List<AccountEntity> tmp = await context.Account.Where(s => s.MasterAccountId == masterAccountId).ToListAsync();
                return _mapper.Map(tmp);
            }
            catch (Exception e)
            {
                Log.Error("WriteGeneralLog", e);
                return new List<AccountDTO>();
            }
        }

        public async Task<IEnumerable<AccountDTO>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<AccountDTO> GetByIdAsync(long id) => await _repository.GetByIdAsync(id);

        public async Task<IEnumerable<AccountDTO>> GetByIdsAsync(IEnumerable<long> ids) => await _repository.GetByIdsAsync(ids);

        public async Task<AccountDTO> SaveAsync(AccountDTO obj) => await _repository.SaveAsync(obj);

        public async Task<IEnumerable<AccountDTO>> SaveAsync(IReadOnlyList<AccountDTO> objs) => await _repository.SaveAsync(objs);

        public async Task DeleteByIdAsync(long id)
        {
            await _repository.DeleteByIdAsync(id);
        }

        public async Task DeleteByIdsAsync(IEnumerable<long> ids)
        {
            await _repository.DeleteByIdsAsync(ids);
        }
    }
}