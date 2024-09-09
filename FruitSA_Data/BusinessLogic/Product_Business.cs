using AutoMapper;
using FruitSA_Data.Context;
using FruitSA_DataAccess.BusinessLogic.IBusinessLogic;
using FruitSA_Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                await _context.SaveChangesAsync();

                return 1;
            }
            return 0;
        }

        public async Task<ProductDTO> Get(Expression<Func<Product, bool>> filter, string? includeProperties = null, bool tracked = true)
        {
            // Use tracking or no-tracking based on the 'tracked' parameter
            IQueryable<Product> query = tracked ? _context.Products : _context.Products.AsNoTracking();

            // Apply the provided filter expression
            query = query.Where(filter);

            // Include related entities if specified
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            // Fetch the product that matches the filter
            var obj = await query.FirstOrDefaultAsync();

            // Map and return the result as ProductDTO
            if (obj != null)
            {
                return _autoMapper.Map<Product, ProductDTO>(obj);
            }

            // Return a new ProductDTO if no product is found
            return new ProductDTO();
        }


        public async Task<IEnumerable<ProductDTO>> GetAll(Expression<Func<Product, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable<Product> query = _context.Products;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            var products = await query.ToListAsync();
            return _autoMapper.Map<IEnumerable<ProductDTO>>(products);
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

                if (objDTO.ImagePath != null)
                {
                    objFromDb.ImagePath = objDTO.ImagePath;
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
