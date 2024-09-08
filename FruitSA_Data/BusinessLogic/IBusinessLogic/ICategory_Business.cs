using FruitSA_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FruitSA_DataAccess.BusinessLogic.IBusinessLogic
{
    public interface ICategory_Business
    {
        public Task<CategoryDTO> Create(CategoryDTO objDTO);
        public Task<CategoryDTO> Update(CategoryDTO objDTO);
        public Task<CategoryDTO> GetByCategoryCode(string categoryCode);
        public Task<int> Delete(int id);
        public Task<CategoryDTO> Get(int id, string name = null, bool? isActive = null);
        public Task<IEnumerable<CategoryDTO>> GetAll(string name = null, bool? isActive = null);
    }
}
