using FruitSA_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FruitSA_DataAccess.BusinessLogic.IBusinessLogic
{
    public interface IProduct_Business
    {
        public Task<ProductDTO> Create(ProductDTO objDTO);
        public Task<ProductDTO> Update(ProductDTO objDTO);
        public Task<int> Delete(int id);
        public Task<ProductDTO> Get(Expression<Func<Product, bool>> filter, string? includeProperties = null, bool tracked = true);
        public Task<IEnumerable<ProductDTO>> GetAll(Expression<Func<Product, bool>>? filter = null, string? includeProperties = null);

    }
}
