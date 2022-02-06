using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace StockPredictor.DataRetriever.Services.ApplicationServices
{
    public class Map<TEntity, TNaturalKey, TKey>
        where TEntity : class
    {
        private readonly Func<TNaturalKey, TNaturalKey> keyTransform;
        private readonly Func<TEntity, TNaturalKey> naturalKeySelector;
        private readonly Func<TEntity, TKey> keySelector;

        private readonly Dictionary<TNaturalKey, TEntity> dictionaryByNaturalKey =
            new Dictionary<TNaturalKey, TEntity>();
        
        public Map(Func<TEntity, TNaturalKey> naturalKeySelector, Func<TEntity, TKey> keySelector)
        {
            keyTransform = x => x;
            this.naturalKeySelector = naturalKeySelector;
            this.keySelector = keySelector;
        }

        public async Task LoadAsync(IQueryable<TEntity> query)
        {
            foreach (var entity in await query.AsNoTracking().ToListAsync())
            {
                dictionaryByNaturalKey[keyTransform(naturalKeySelector(entity))] = entity;
            }
        }

        public TKey MapOnly(TNaturalKey naturalKey)
        {
            return keySelector(dictionaryByNaturalKey[keyTransform(naturalKey)]);
        }

        public async Task<TKey> MapOrCreate(TNaturalKey naturalKey, Func<Task<TEntity>> create)
        {
            if (dictionaryByNaturalKey.TryGetValue(keyTransform(naturalKey), out var entity))
                return keySelector(entity);

            entity = await create();
            dictionaryByNaturalKey[keyTransform(naturalKey)] = entity;
            return keySelector(entity);
        }
    }
}