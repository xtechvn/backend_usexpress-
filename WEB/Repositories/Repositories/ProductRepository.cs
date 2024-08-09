using Caching.Elasticsearch;
using DAL;
using Elasticsearch.Net;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Options;
using Nest;
using Repositories.IRepositories;
using Repositories.ShippingFeeRepositories.LabelFee;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories
{
    /// <summary>
    /// Repos này sẽ là lớp BASE của Product
    /// 1. Single responsibility principle: Mỗi đối tượng chỉ làm duy nhất 1 nhiệm vụ
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        // private readonly EsConnection es_repository;
        private readonly ProductDAL _ProductDAL;
        private readonly GroupProductDAL _GroupProductDAL;
        private readonly SpecialIndustryDAL _SpecialIndustryDAL;
        private readonly IOptions<DataBaseConfig> _DataBaseConfig;

        public ProductRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _DataBaseConfig = dataBaseConfig;
            _ProductDAL = new ProductDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _GroupProductDAL = new GroupProductDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _SpecialIndustryDAL = new SpecialIndustryDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public async Task<long> Create(ProductViewModel model)
        {
            try
            {
                var entities = new Product()
                {
                    ProductCode = model.product_code,
                    Title = model.product_name,
                    Price = model.price,
                    //Discount = model.discount,
                    Amount = model.amount,
                    Rating = model.rating,
                    Manufacturer = model.manufacturer,
                    LabelId = model.label_id,
                    //ReviewsCount = !string.IsNullOrEmpty(model.reviews_count) ? int.Parse(model.reviews_count.Trim()) : 0,
                    IsPrimeEligible = model.is_prime_eligible,
                    SellerId = model.seller_id,
                    SellerName = model.seller_name,
                    CreateOn = DateTime.Now,
                    GroupProductId = -1,
                    Variations = model.variations,
                    ProductMapId = model.product_map_id,
                    LinkSource = model.link_product
                };
                var rs = await _ProductDAL.CreateAsync(entities);
                return rs;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Create - ProductRepository: " + ex);
                return -1;
            }
        }

        public Task<int> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<Product> GetAll()
        {
            return _ProductDAL.GetAll();
        }

        public Task<Product> GetById(int Id)
        {
            return _ProductDAL.GetById(Id);
        }

        public Task<Product> GetByProductMapId(int productMapId)
        {
            return _ProductDAL.GetByProductMapId(productMapId);
        }

        public Task<Product> GetByProductCode(string productCode, int labelId)
        {
            return _ProductDAL.GetByProductCode(productCode, labelId);
        }

        public Task<Product> GetByProductCode(string productCode)
        {
            return _ProductDAL.GetByProductCode(productCode);
        }

        public async Task<long> Update(ProductViewModel model)
        {
            try
            {
                var entities = await _ProductDAL.FindAsync(model.id);
                if (entities != null)
                {
                    entities.ProductCode = model.product_code;
                    entities.Title = model.product_name;
                    entities.Price = model.price;
                    //Discount = model.discount,
                    entities.Amount = model.amount;
                    entities.Rating = model.rating;
                    entities.Manufacturer = model.manufacturer;
                    entities.LabelId = model.label_id;
                    entities.ReviewsCount = model.reviews_count;
                    entities.IsPrimeEligible = model.is_prime_eligible;
                    entities.SellerId = model.seller_id;
                    entities.SellerName = model.seller_name;

                    entities.CreateOn = DateTime.Now;
                    entities.GroupProductId = -1;
                    entities.Variations = model.variations;
                    entities.ProductMapId = model.product_map_id;
                    var rs = await _ProductDAL.UpdateItem(entities);
                    return rs;
                }
                return -1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - ProductRepository: " + ex);
                return -1;
            }
        }

        /// <summary>
        /// CREATE BY: CUONGLV
        /// NEED: Tính ra phí mua hộ của sản phẩm theo các nhãn
        /// </summary>
        /// <param name="label_id">Id của nhãn hàng</param>
        /// <param name="product_buyer">Input chứa thông tin của sản phẩm là đầu vào để tính phí</param>
        /// <returns></returns>
        public async Task<Dictionary<string, double>> getShippingFee(int label_id, ProductBuyerViewModel product_buyer)
        {
            try
            {
                var shipping_fee = new Dictionary<string, double>();

                // Lấy ra các mức phí của 1 nhãn
                var price_level = await _ProductDAL.getPriceLevelByLabelId(label_id);

                // Phân loại ngành hàng đặc biệt khi đơn giá sp vượt mức cho 1 mức phí Luxury
                var industry_special = await _SpecialIndustryDAL.getSpecicalLuxuryBySpecialType(product_buyer.IndustrySpecialType);

                // Tính phí mua hộ theo từng store
                switch (label_id)
                {
                    case (int)LabelType.amazon:
                        var calulator_amz = new FeeAmazon(product_buyer, price_level, industry_special);
                        shipping_fee = calulator_amz.getAmazoneShippingFee();
                        break;
                    case (int)LabelType.costco:
                        var calulator_cc = new FeeCostco(product_buyer, price_level, industry_special);
                        shipping_fee = calulator_cc.getCostcoShippingFee();
                        break;
                    case (int)LabelType.jomashop:
                        var calulator_jms = new FeeJomashop(product_buyer, price_level, industry_special);
                        shipping_fee = calulator_jms.getJomashopShippingFee();
                        break;
                    case (int)LabelType.bestbuy:
                        var calulator_bb = new FeeBestBuy(product_buyer, price_level, industry_special);
                        shipping_fee = calulator_bb.getBestBuyShippingFee();
                        break;
                    case (int)LabelType.sephora:
                        var calulator_spr = new FeeSephora(product_buyer, price_level, industry_special);
                        shipping_fee = calulator_spr.getSephoraShippingFee();
                        break;
                    case (int)LabelType.nordstromrack:
                        var calulator_nr = new FeeNordstromRack(product_buyer, price_level, industry_special);
                        shipping_fee = calulator_nr.getNordstromRackShippingFee();
                        break;
                    case (int)LabelType.hautelook:
                        var calulator_htl = new FeeHautelook(product_buyer, price_level, industry_special);
                        shipping_fee = calulator_htl.getHautelookShippingFee();
                        break;
                    case (int)LabelType.victoria_secret:
                        var calulator_vc = new FeeVictoriaSecret(product_buyer, price_level, industry_special);
                        shipping_fee = calulator_vc.getVictoriaSecretShippingFee();
                        break;
                }

                return shipping_fee;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Product - getShippingFee: " + ex);
                return null;
            }
        }

        public async Task<GenericViewModel<ProductViewModel>> GetProductPagingList(ProductFilterModel filter)
        {
            try
            {
                var model = new GenericViewModel<ProductViewModel>();

                var nodes = new Uri[] { new Uri(_DataBaseConfig.Value.Elastic.Host) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var Clauses = new List<QueryContainer>();
                var sort_clauses = new List<ISort>();

                if (filter.SortField == 0)
                {
                    sort_clauses.Add(new FieldSort { Field = "update_last", Order = (SortOrder)filter.SortType });
                }
                else if (filter.SortField == 1)
                {
                    sort_clauses.Add(new FieldSort { Field = "amount_vnd", Order = (SortOrder)filter.SortType });
                }
                else if (filter.SortField == 2)
                {
                    sort_clauses.Add(new FieldSort { Field = "product_bought_quantity", Order = (SortOrder)filter.SortType });
                }
                else if (filter.SortField == 3)
                {
                    sort_clauses.Add(new FieldSort { Field = "shiping_fee", Order = (SortOrder)filter.SortType });
                }

                if (!string.IsNullOrEmpty(filter.ProductName))
                {
                    if (!filter.ProductName.Contains(" "))
                    {
                        Clauses.Add(new BoolQuery
                        {
                            Should = new List<QueryContainer>
                            {
                                new WildcardQuery
                                {
                                      Field = new Field("product_name.keyword"),
                                      Value = "*" + filter.ProductName.ToUpper() + "*"
                                },
                                new MatchQuery
                                {
                                    Field = new Field("product_name"),
                                    Query = filter.ProductName
                                }
                            }
                        });
                    }
                    else
                    {
                        Clauses.Add(new MatchPhraseQuery
                        {
                            Field = new Field("product_name"),
                            Analyzer = "standard",
                            Boost = 1.1,
                            Name = "product_name_query",
                            Query = filter.ProductName,
                            Slop = 2
                        });
                    }
                }

                if (!string.IsNullOrEmpty(filter.ProductCode))
                {
                    if (!filter.ProductCode.Contains(" "))
                    {
                        Clauses.Add(new WildcardQuery
                        {
                            Field = new Field("product_code.keyword"),
                            Value = "*" + filter.ProductCode.ToUpper() + "*"
                        });
                    }
                    else
                    {
                        Clauses.Add(new MatchQuery
                        {
                            Field = new Field("product_code"),
                            Query = filter.ProductCode
                        });
                    }
                }

                if (filter.Labels != null && filter.Labels.Count() > 0)
                {
                    Clauses.Add(new TermsQuery
                    {
                        Field = new Field("label_id"),
                        Terms = filter.Labels.Select(s => s.ToString())
                    });
                }

                if (filter.Categories != null && filter.Categories.Count() > 0)
                {
                    Clauses.Add(new TermsQuery
                    {
                        Field = new Field("group_product_id"),
                        Terms = filter.Categories.Select(s => s.ToString())
                    });
                }

                if (filter.FromDate != DateTime.MinValue && filter.ToDate != DateTime.MinValue)
                {
                    Clauses.Add(new DateRangeQuery
                    {
                        Field = new Field("create_date"),
                        GreaterThanOrEqualTo = filter.FromDate,
                        LessThanOrEqualTo = filter.ToDate.AddDays(1).AddSeconds(-1)
                    });
                }

                if (filter.Status > 0)
                {
                    var page_not_found = false;
                    if (filter.Status == 2)
                    {
                        page_not_found = true;
                    }

                    Clauses.Add(new TermQuery
                    {
                        Field = new Field("page_not_found"),
                        Value = page_not_found
                    });
                }

                var _SearchRequest = new SearchRequest<ProductViewModel>("product")
                {
                    Query = new BoolQuery { Must = Clauses },
                    From = (filter.PageIndex - 1) * filter.PageSize,
                    Size = filter.PageSize,
                    Sort = sort_clauses
                };

                Func<CountDescriptor<ProductViewModel>, CountRequest> _CountRequest = q => new CountRequest<ProductViewModel>("product")
                {
                    Query = new BoolQuery { Must = Clauses },
                };

                var searchResponse = await elasticClient.SearchAsync<ProductViewModel>(_SearchRequest);
                var countResponse = await elasticClient.CountAsync<ProductViewModel>(_CountRequest);

                model.CurrentPage = filter.PageIndex;
                model.PageSize = filter.PageSize;
                model.TotalRecord = countResponse.Count;
                model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                model.ListData = searchResponse.Documents.ToList();

                return model;
            }
            catch
            {
                return null;
            }
        }

        public object GetListBoughtProductQuantity(string arrProductCode)
        {
            return _ProductDAL.GetDataTableBoughtProductQuantity(arrProductCode);
        }

        public async Task<bool> UpdateProductQuantityAndGroupOnElastic(IEnumerable<ESModifyProductModel> ArrProduct)
        {
            try
            {
                IESRepository<object> _ESRepository = new ESRepository<object>(_DataBaseConfig.Value.Elastic.Host);

                var arrProductCode = string.Join(",", ArrProduct.Select(s => s.product_code));
                var dataTableQuantity = _ProductDAL.GetDataTableBoughtProductQuantity(arrProductCode);

                var ProductQuantityInfoList = dataTableQuantity.AsEnumerable().Select(row => new
                {
                    ProductCode = row["ProductCode"].ToString(),
                    Quantity = !row["Quantity"].Equals(DBNull.Value) ? long.Parse(row["Quantity"].ToString()) : 0
                });


                foreach (var item in ArrProduct)
                {
                    var product = _ESRepository.getProductDetailByCode("product", item.product_code, item.label_id);
                    if (product != null)
                    {
                        var ProductQuantityInfoModel = ProductQuantityInfoList.Where(s => s.ProductCode == item.product_code).FirstOrDefault();
                        product.product_bought_quantity = ProductQuantityInfoModel != null ? ProductQuantityInfoModel.Quantity : 0;

                        string group_name = string.Empty;
                        if (product.group_product_id > 0)
                        {
                            var groupModel = await _GroupProductDAL.FindAsync(product.group_product_id);
                            group_name = groupModel != null ? groupModel.Name : string.Empty;
                        }

                        product.group_product_name = group_name;
                        _ESRepository.UpSert(product, "product");
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        public DataTable GetInterestedProduct(int PageIndex, int PageSize)
        {
            return  _ProductDAL.GetDataTableInterestedProduct(PageIndex, PageSize);
        }
        public DataTable GetInterestedProductTotalRecord()
        {
            return _ProductDAL.GetDataTableInterestedProductTotalRecord();
        }
    }
}
