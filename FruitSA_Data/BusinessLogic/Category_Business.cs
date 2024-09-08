using AutoMapper;
using FruitSA_Data.Context;
using FruitSA_DataAccess.BusinessLogic.IBusinessLogic;
using FruitSA_Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FruitSA_DataAccess.BusinessLogic
{
    public class Category_Business : ICategory_Business
    {
        private readonly ApplicationDbContext _context;
        internal DbSet<Category> dbSet;
        private readonly IMapper _autoMapper;

        public Category_Business(ApplicationDbContext db, IMapper mapper)
        {
            this.dbSet = dbSet;
            _context = db;
            _autoMapper = mapper;
        }

        public async Task<CategoryDTO> Create(CategoryDTO objDTO)
        {
            var obj = _autoMapper.Map<CategoryDTO, Category>(objDTO);
            obj.CreatedAt = DateTime.Now;

            var addedObj = _context.Categories.Add(obj);
            await _context.SaveChangesAsync();

            return _autoMapper.Map<Category, CategoryDTO>(addedObj.Entity);
        }

        public async Task<int> Delete(int id)
        {
            var obj = await _context.Categories.FirstOrDefaultAsync(u => u.CategoryId == id);
            if (obj != null)
            {
                _context.Categories.Remove(obj);
                return await _context.SaveChangesAsync();
            }
            return 0;
        }

        // Get method with filtering options
        public async Task<CategoryDTO> Get(int id, string name = null, bool? isActive = null)
        {
            // Start by fetching category by id
            IQueryable<Category> query = _context.Categories.Where(c => c.CategoryId == id);

            // Apply filtering by name if provided
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => c.Name.Contains(name));
            }

            // Apply filtering by IsActive status if provided
            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            // Get the category
            var obj = await query.FirstOrDefaultAsync();

            // Map and return the result
            if (obj != null)
            {
                return _autoMapper.Map<Category, CategoryDTO>(obj);
            }
            return new CategoryDTO();
        }
        public async Task<CategoryDTO> GetByCategoryCode(string categoryCode)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryCode == categoryCode);
            return _autoMapper.Map<CategoryDTO>(category);
        }

        // GetAll method with filtering options
        public async Task<IEnumerable<CategoryDTO>> GetAll(string name = null, bool? isActive = null)
        {
            // Start with all categories
            IQueryable<Category> query = _context.Categories;

            // Apply filtering by name if provided
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => c.Name.Contains(name));
            }

            // Apply filtering by IsActive status if provided
            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            // Execute the query and map the results
            var obj = await query.ToListAsync();
            return _autoMapper.Map<IEnumerable<CategoryDTO>>(obj);
        }

        public async Task<CategoryDTO> Update(CategoryDTO objDTO)
        {
            var objFromDb = await _context.Categories.FirstOrDefaultAsync(u => u.CategoryId == objDTO.CategoryId);

            if (objFromDb != null)
            {
                objFromDb.Name = objDTO.Name;
                objFromDb.IsActive = objDTO.IsActive;
                objFromDb.Username = objDTO.Username;
                objFromDb.CategoryCode = objDTO.CategoryCode;
                objFromDb.UpdateAt = DateTime.Now;
                _context.Categories.Update(objFromDb);
                await _context.SaveChangesAsync();

                return _autoMapper.Map<Category, CategoryDTO>(objFromDb);
            }
            return objDTO;
        }
    }
}
