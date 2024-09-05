using AutoMapper;
using FruitSA_Data.Context;
using FruitSA_DataAccess.BusinessLogic.IBusinessLogic;
using FruitSA_Models;
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
        private readonly IMapper _autoMapper;
        public Category_Business(ApplicationDbContext db, IMapper mapper)
        {
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



        public async Task<CategoryDTO> Get(int id)
        {
            var obj = await _context.Categories.FirstOrDefaultAsync(u => u.CategoryId == id);



            if (obj != null)
            {
                return _autoMapper.Map<Category, CategoryDTO>(obj);
            }
            return new CategoryDTO();
        }
        public async Task<IEnumerable<CategoryDTO>> GetAll()
        {
            var obj = await _context.Categories.ToListAsync();
            return _autoMapper.Map<IEnumerable<CategoryDTO>>(obj);
        }



        public async Task<CategoryDTO> Update(CategoryDTO objDTO)
        {
            var objFromDb = await _context.Categories.FirstOrDefaultAsync(u => u.CategoryId == objDTO.CategoryId);
            if (objFromDb != null)
            {
                objFromDb.Name = objDTO.Name;
                objFromDb.UpdateAt = DateTime.Now;
                _context.Categories.Update(objFromDb);
                await _context.SaveChangesAsync();



                return _autoMapper.Map<Category, CategoryDTO>(objFromDb);
            }
            return objDTO;
        }
    }
}
