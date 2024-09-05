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
    public class Product_Business : IProduct_Business
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _autoMapper;
        public Product_Business(ApplicationDbContext db, IMapper mapper)
        {
            _context = db;
            _autoMapper = mapper;
        }
        public async Task<ProductDTO> Create(ProductDTO objDTO)
        {
            var obj = _autoMapper.Map<ProductDTO, Product>(objDTO);
            obj.CreatedAt = DateTime.Now;

            var addedObj = _context.Products.Add(obj);
            await _context.SaveChangesAsync();

            return _autoMapper.Map<Product, ProductDTO>(addedObj.Entity);
        }



        public async Task<int> Delete(int id)
        {
            var obj = await _context.Products.FirstOrDefaultAsync(u => u.ProductId == id);
            if (obj != null)
            {
                _context.Products.Remove(obj);
                return await _context.SaveChangesAsync();
            }
            return 0;
        }



        public async Task<ProductDTO> Get(int id)
        {
            var obj = await _context.Products.FirstOrDefaultAsync(u => u.ProductId == id);



            if (obj != null)
            {
                return _autoMapper.Map<Product, ProductDTO>(obj);
            }
            return new ProductDTO();
        }
        public async Task<IEnumerable<ProductDTO>> GetAll()
        {
            var obj = await _context.Products.ToListAsync();
            return _autoMapper.Map<IEnumerable<ProductDTO>>(obj);
        }



        public async Task<ProductDTO> Update(ProductDTO objDTO)
        {
            var objFromDb = await _context.Products.FirstOrDefaultAsync(u => u.ProductId == objDTO.ProductId);
            if (objFromDb != null)
            {
                objFromDb.ProductName = objDTO.ProductName;
                objFromDb.ProductCode = objDTO.ProductCode;
                objFromDb.Price = objDTO.Price;
                objFromDb.Description = objDTO.Description;
                objFromDb.Username = objDTO.Username;
                objFromDb.CategoryId = objDTO.CategoryId;

                if (objFromDb.ImagePath != null)
                {
                    objFromDb.ImagePath = objFromDb.ImagePath;
                }

                objFromDb.UpdateAt = DateTime.Now;
                _context.Products.Update(objFromDb);
                await _context.SaveChangesAsync();



                return _autoMapper.Map<Product, ProductDTO>(objFromDb);
            }
            return objDTO;
        }
    }
}
