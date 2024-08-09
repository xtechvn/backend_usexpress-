using Elasticsearch.Net;
using Entities.ViewModels;
using Entities.ViewModels.Product;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace Caching.Elasticsearch
{
    //https://www.steps2code.com/post/how-to-use-elasticsearch-in-csharp
    public class ESRepository<TEntity> : IESRepository<TEntity> where TEntity : class
    {
        private static string _ElasticHost;

        public ESRepository(string Host)
        {
            _ElasticHost = Host;
        }

        public bool DeleteProductByCode(string index_name, string document_id)
        {
            string key_es_id = string.Empty;
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                // input
                //var document = new ProductViewModel
                //{
                //    product_code = document_id
                //};
                //// find key
                //var result = elasticClient.Index(document, i => i.Index(index_name));
                /*
                var result = elasticClient.Search<object>(s => s
                                             .Index(index_name)
                                             .Query(q => q
                                             .Term("product_code", document_id))
                                             );*/
                var result = elasticClient.Search<object>(sd => sd
                               .Index(index_name)
                               .Query(q => q
                                   .Match(m => m.Field("product_code").Query(document_id)
                                   )));

                if (result.IsValid && result.Documents.Count > 0)
                {
                    foreach (Hit<object> hit in (IHit<object>[])result.HitsMetadata.Hits)
                    {
                        key_es_id = ((Hit<object>)((IHit<object>[])result.HitsMetadata.Hits)[0]).Id;
                        var response = elasticClient.Delete<object>(key_es_id.ToString(), d => d
                        .Index(index_name)
                        );
                    }
                    return result.IsValid;
                }
                else
                {
                    return true; // chưa có mã này
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteProductByCode key_es_id= " + key_es_id + " document_id" + document_id + " Exception" + ex.ToString());
                return false;
            }
        }

        public async Task<bool> DeleteProductByKey(string index_name, string product_code, int label_id)
        {
            string key_es_id = string.Empty;
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                //var result = elasticClient.Search<ProductViewModel>(s => s
                //                    .Index(index_name)
                //                    .Query(q => q
                //                        .Match(m => m
                //                        .Field(f => f.product_code)
                //                        .Query(product_code)
                //                        )
                //                    )
                //                    .Aggregations(a => a
                //                    .Terms(label_id.ToString(), t => t
                //                    .Field(f => f.label_id)
                //                    )
                //                    )
                //               );



                var filters = new List<Func<QueryContainerDescriptor<ProductViewModel>, QueryContainer>>();

                filters.Add(fq => fq.Terms(t => t.Field(f => f.product_code).Terms<string>(product_code))); // theo code
                var label_list = new[] { label_id.ToString() };
                filters.Add(fq => fq.Terms(c => c.Field(f => f.label_id).Terms(label_list))); // filter theo label

                var result = await elasticClient.SearchAsync<ProductViewModel>(s => s
                       .Index(index_name)
                       .Sort(x => x.Descending(u => u.update_last))
                       .Query(q => q.Bool(bq => bq.Filter(filters)))
                );


                

                if (result.IsValid && result.Documents.Count > 0)
                {
                    for (int i = 0; i <= result.HitsMetadata.Hits.Count - 1; i++)
                    {
                        key_es_id = ((Nest.IHit<Entities.ViewModels.ProductViewModel>[])((IHit<object>[])result.HitsMetadata.Hits))[i].Id;
                        var response = elasticClient.Delete<object>(key_es_id.ToString(), d => d
                              .Index(index_name)
                              );
                    }
                    return result.IsValid;
                }
                else
                {
                    LogHelper.InsertLogTelegram("DeleteProductByCode product_code= " + product_code + " chua co ma nay");
                    return true; // chưa có mã này
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteProductByCode product_code= " + product_code + " label_id " + label_id + " Exception" + ex.ToString());
                return false;
            }
        }
        /// <summary>

        /// <summary>

        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="value">Giá trị cần tìm kiếm</param>
        /// <param name="field_name">Tên cột cần search</param>
        /// <returns></returns>
        public TEntity FindById(string indexName, object value, string field_name = "id")
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var searchResponse = elasticClient.Search<object>(s => s
                    .Index(indexName)
                    .Query(q => q.Term(field_name, value))
                );

                var JsonObject = JsonConvert.SerializeObject(searchResponse.Documents);
                var ListObject = JsonConvert.DeserializeObject<List<TEntity>>(JsonObject);
                return ListObject.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Lấy ra chi tiết sản phẩm theo code
        /// </summary>
        /// <param name="index_name"></param>
        /// <param name="value_search"></param>
        /// <returns></returns>
        public ProductViewModel getProductDetailByCode(string index_name, string product_code, int label_id)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                //var search_response = eClient.Search<ProductViewModel>(s => s
                //      .Index(index_name)
                //      .Size(top)
                //      .Query(q => q
                //         .Match(m => m
                //         .Field(f => f.product_name)
                //         .Query(input_search)
                //         )
                //      ));

                var search_response = elasticClient.Search<ProductViewModel>(s => s
                                    .Index(index_name)
                                    .Query(q => q
                                        .Match(m => m
                                        .Field(f => f.product_code)
                                        .Query(product_code)
                                        )
                                    )
                                    .Aggregations(a => a
                                    .Terms(label_id.ToString(), t => t
                                    .Field(f => f.label_id)
                                    )
                                    )
                               );

                if (!search_response.IsValid)
                {
                    // LogHelper.InsertLogTelegram("1264151832:AAFKCM7CoaBerVhcYCHQba1AyY13X41rT5s", "-406438822", "Keyword " + input_search + " khong ton tai trong ES");
                }
                else
                {
                    if (search_response.Documents.Count > 0)
                    {
                        return (search_response.Documents as List<ProductViewModel>)[0];
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("1264151832:AAFKCM7CoaBerVhcYCHQba1AyY13X41rT5s", "-406438822", "ex getProductDetailByCode " + ex.ToString());
                return null;
            }
        }

        public int UpSert(TEntity entity, string indexName)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var indexResponse = elasticClient.Index(new IndexRequest<TEntity>(entity, indexName));

                if (!indexResponse.IsValid)
                {
                    LogHelper.WriteLogActivity(Directory.GetCurrentDirectory(), indexResponse.OriginalException.Message);
                    LogHelper.WriteLogActivity(Directory.GetCurrentDirectory(), "apicall" + indexResponse.ApiCall.OriginalException.Message);
                    return 0;
                }

                return 1;
            }
            catch
            {
                return -1;
            }
        }

        public async Task<int> UpSertAsync(TEntity entity, string indexName)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var indexResponse = elasticClient.Index(entity, i => i.Index(indexName));
                if (!indexResponse.IsValid)
                {
                    // If the request isn't valid, we can take action here
                }

                var indexResponseAsync = await elasticClient.IndexDocumentAsync(entity);
            }
            catch
            {

            }
            return 0;
        }

        public int UpSertMultiple(IEnumerable<TEntity> entity, string indexName)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpSertMultipleAsync(IEnumerable<TEntity> entity, string indexName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// CUONGLV
        /// Tìm kiếm sản phẩm. Truy Vấn data trên ES
        /// https://www.elastic.co/guide/en/elasticsearch/client/net-api/7.x/writing-queries.html
        /// https://francescopantisano.it/elasticsearch-5-start-using-nest-c-net/
        /// </summary>
        /// <returns></returns>
        public List<ProductViewModel> searchProduct(string input_search, string index_name, int top)
        {
            string msg_es_respon = string.Empty;
            try
            {
                // Connection URL's ES
                var es = new EsConnection(_ElasticHost);
                var eClient = es.connectES(index_name, out msg_es_respon);

                if (eClient == null)
                {
                    // LogHelper.InsertLogTelegram("1372498309:AAH0fVJfnZQFg5Qaqro47y1o5mIIcwVkR3k", "-309075192", "CACHING-SearchProduct:" + msg_es_respon);
                    return null;
                }

                //Query execute data
                #region Search all field in document
                //var search_response = eClient.Search<ProductViewModel>(s => s
                //.StoredFields(sf => sf
                //    .Fields(
                //        f => f.title,
                //        f => f.keywords
                //    )
                //)
                //.Query(q => q
                //.Term("title", input_search)
                //)
                //);
                #endregion

                var search_response = eClient.Search<ProductViewModel>(s => s
                       .Index(index_name)
                       .Size(top)
                       .Query(q => q
                          .Match(m => m
                          .Field(f => f.product_name)
                          .Query(input_search)
                          )
                       ));

                if (!search_response.IsValid)
                {
                    LogHelper.InsertLogTelegram("1264151832:AAFKCM7CoaBerVhcYCHQba1AyY13X41rT5s", "-406438822", "Keyword " + input_search + " khong ton tai trong ES");
                }
                else
                {
                    if (search_response.Documents.Count > 0)
                    {
                        return search_response.Documents as List<ProductViewModel>;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                // LogHelper.InsertLogTelegram("1264151832:AAFKCM7CoaBerVhcYCHQba1AyY13X41rT5s", "-406438822", "rs =" + "CACHING-SearchProduct:" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// from: số bản ghi bỏ qua
        /// size: số bản ghi lấy
        /// </summary>
        /// <param name="index_name"></param>
        /// <param name="grp_id"></param>
        /// <param name="from"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public async Task<SearchEsEntitiesViewModel> getProductListByGroupProductId(string index_name, List<int> grp_id, int from, int size, string filter = "filter")
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var product_result_model = new SearchEsEntitiesViewModel();

                var filters = new List<Func<QueryContainerDescriptor<ProductViewModel>, QueryContainer>>();
                filters.Add(fq => fq.Terms(t => t.Field(f => f.group_product_id).Terms<int>(grp_id)));
                filters.Add(fq => fq.Range(c => c.Field(f => f.amount_vnd).GreaterThan(0)));
                //if (filter != "all")
                //{
                //    filters.Add(fq => fq.Range(c => c.Field(f => f.discount).GreaterThan(0)));
                //}
                var search_response_mul = await elasticClient.SearchAsync<ProductViewModel>(s => s
                       .Index(index_name)
                       .From(from)
                       .Size(size)
                       .Sort(x => x.Descending(u => u.update_last))
                       .Query(q => q.Bool(bq => bq.Filter(filters)))
                );



                if (!search_response_mul.IsValid)
                {
                    LogHelper.InsertLogTelegram("1264151832:AAFKCM7CoaBerVhcYCHQba1AyY13X41rT5s", "-406438822", "getProductListByGroupProductId grp_id = " + JsonConvert.SerializeObject(grp_id) + " khong ton tai trong ES");
                }
                else
                {
                    product_result_model = new SearchEsEntitiesViewModel
                    {
                        obj_lst_product_result = search_response_mul.Documents as List<ProductViewModel>,
                        total_item_store = search_response_mul.Total
                    };

                    if (product_result_model.obj_lst_product_result.Count == 0)
                    {
                        LogHelper.InsertLogTelegram("1264151832:AAFKCM7CoaBerVhcYCHQba1AyY13X41rT5s", "-406438822", "error ES getProductListByGroupProductId response_es = " + JsonConvert.SerializeObject(search_response_mul));
                    }
                }


                return product_result_model.total_item_store == 0 ? null : product_result_model;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("1264151832:AAFKCM7CoaBerVhcYCHQba1AyY13X41rT5s", "-406438822", "ex getProductListByGroupProductId " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Lấy ra những mã sp không tồn tài trong ES
        /// </summary>
        /// <param name="index_name"></param>
        /// <param name="lst_product_code_target"></param>
        /// <returns></returns>
        public async Task<string> getListProductCodeNotExits(string index_name, List<string> lst_product_code_target, int group_id)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var product_result_model = new SearchEsEntitiesViewModel();

                var search_response_mul = await elasticClient.SearchAsync<object>(s => s
                       .Index(index_name)
                       .Sort(x => x.Descending("create_date"))
                       .From(0)
                       .Size(1000)
                       .Query(q => q
                            .TermsSet(t => t
                            .Field("product_code")
                            .Terms(lst_product_code_target.ToArray())
                        ) && q
                        .Match(m => m
                            .Field("group_product_id")
                            .Query(group_id.ToString())
                        )));

                if (!search_response_mul.IsValid)
                {
                    LogHelper.InsertLogTelegramByUrl("1372498309:AAH0fVJfnZQFg5Qaqro47y1o5mIIcwVkR3k", "-309075192", "api:getListProductCodeNotExits querry error : ");
                }
                else
                {
                    var JsonObject = JsonConvert.SerializeObject(search_response_mul.Documents);
                    var lst_product_code_exits = JsonConvert.DeserializeObject<List<ProductESViewModel>>(JsonObject);
                    //  var lst_product_code_exits = search_response_mul.Documents as List<ProductViewModel>;

                    if (lst_product_code_exits.Count > 0)
                    {
                        var list_not_exit = from n in lst_product_code_target
                                            where !(lst_product_code_exits.Select(x => x.product_code).Contains(n))
                                            select n;
                        // LogHelper.InsertLogTelegramByUrl("1372498309:AAH0fVJfnZQFg5Qaqro47y1o5mIIcwVkR3k", "-309075192", "------------api:search_response_mul : " + JsonConvert.SerializeObject(lst_product_code_exits.Select(x => x.product_code)) + "----------- lst_product_code_target =" + JsonConvert.SerializeObject(lst_product_code_target) + "-----------list_not_exit: " + JsonConvert.SerializeObject(list_not_exit));
                        return string.Join(",", list_not_exit); // respon ra nhung ma san pham ko co trong he thong
                    }
                }
                return string.Join(",", lst_product_code_target);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("1372498309:AAH0fVJfnZQFg5Qaqro47y1o5mIIcwVkR3k", "-309075192", "es--getListProductCodeNotExits = " + ex.ToString() + "== " + string.Join(",", lst_product_code_target) + "__groupproductid " + group_id);
                return string.Join(",", lst_product_code_target);
            }
        }
        public long GetIdentityProductEs(string index_name)
        {
            string msg_es_respon = string.Empty;
            var rd = new Random();
            var id_tmp = rd.Next(0, 10000000);
            try
            {

                // Connection URL's ES
                var es = new EsConnection(_ElasticHost);
                var eClient = es.connectES(index_name, out msg_es_respon);

                if (eClient == null)
                {
                    return 1;
                }

                var search_response = eClient.Search<ProductViewModel>(s => s
                       .Index(index_name)
                       .Aggregations(x => x
                       .Max("max_commits", x => x.Field(p => p.id))
                       )
                       );

                if (!search_response.IsValid)
                {
                    LogHelper.InsertLogTelegram("1264151832:AAFKCM7CoaBerVhcYCHQba1AyY13X41rT5s", "-406438822", "index_name " + index_name + " countProductEs error");
                }
                else
                {
                    if (search_response.Documents.Count > 0)
                    {
                        return Convert.ToInt64(search_response.Hits.Max(x => Convert.ToInt64(x.Id))) + 1;
                    }
                }
                return id_tmp;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("1264151832:AAFKCM7CoaBerVhcYCHQba1AyY13X41rT5s", "-406438822", "rs =" + "CACHING-countProductEs:" + ex.ToString());
                return id_tmp;
            }
        }

        public int getTotalProductCrawlToday(string index_name, int label_type)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var search_response = elasticClient.Search<ProductViewModel>(s => s
                      .Index(index_name)
                      .Take(2000)
                      .Query(q => q
                         .Bool(b => b
                         .Must(m => m
                            .DateRange(dr => dr
                            .Field(f => f.create_date)
                            .Format("yyyy/MM/dd")
                            .GreaterThanOrEquals(DateTime.Now.ToString("yyyy/MM/dd"))
                             )
                             )
                          )
                      ));

                if (!search_response.IsValid)
                {
                    // LogHelper.InsertLogTelegram("1264151832:AAFKCM7CoaBerVhcYCHQba1AyY13X41rT5s", "-406438822", "Keyword " + input_search + " khong ton tai trong ES");
                }
                else
                {
                    return search_response.Documents.Count;
                }
                return 0;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("1264151832:AAFKCM7CoaBerVhcYCHQba1AyY13X41rT5s", "-406438822", "ex getTotalProductCrawlToday " + ex.ToString());
                return 0;
            }
        }
        public async Task<long> getTotalProductCrawlTodayNewAsync(string index_name)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var Clauses = new List<QueryContainer>();
                Clauses.Add(new DateRangeQuery
                {
                    Field = new Field("create_date"),
                    GreaterThanOrEqualTo = DateTime.Now,
                    LessThanOrEqualTo = DateTime.Now.AddDays(1).AddMilliseconds(-1)
                });

                Func<CountDescriptor<ProductViewModel>, CountRequest> _CountRequest = q => new CountRequest<ProductViewModel>("product")
                {
                    Query = new BoolQuery { Must = Clauses },
                };
                var countResponse = await elasticClient.CountAsync<ProductViewModel>(_CountRequest);

                return countResponse.Count;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("1264151832:AAFKCM7CoaBerVhcYCHQba1AyY13X41rT5s", "-406438822", "ex getTotalProductCrawlNew " + ex.ToString());
                return 0;
            }
        }
        public List<ProductViewModel> GetByGroupID(string index_name, string group_id)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var search_response = elasticClient.Search<ProductViewModel>(s => s
                                    .Index(index_name)
                                    .Query(q => q
                                        .Match(m => m
                                        .Field(f => f.group_product_id)
                                        .Query(group_id)
                                        )
                                    )
                               );
                if (!search_response.IsValid)
                {
                }
                else
                {
                    if (search_response.Documents.Count > 0)
                    {
                        return (search_response.Documents as List<ProductViewModel>);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("1264151832:AAFKCM7CoaBerVhcYCHQba1AyY13X41rT5s", "-406438822", "ex GetByGroupID " + ex.ToString());
            }
            return null;
        }
        public async Task<SearchEsEntitiesViewModel> getProductDetailByProductCodeList(string index_name, List<string> product_code_id, int from, int size, int label_id)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var product_result_model = new SearchEsEntitiesViewModel();

                var filters = new List<Func<QueryContainerDescriptor<ProductViewModel>, QueryContainer>>();
                if (product_code_id.Count() > 0)
                {
                    filters.Add(fq => fq.Terms(t => t.Field(f => f.product_code).Terms<string>(product_code_id))); // theo code
                }


                //var label_list = new[] { label_id.ToString() };
                //filters.Add(fq => fq.Terms(c => c.Field(f => f.label_id).Terms(label_list))); // filter theo label
                //filters.Add(fq => fq.Terms(t => t.Field(f => f.product_status).Terms<int>((int)ProductStatus.NORMAL_SELL))); // sản phẩm đang bán

                //var search_response_mul = await elasticClient.SearchAsync<ProductViewModel>(s => s
                //       .Index(index_name)
                //       .From(from)
                //       .Size(size)
                //       .Sort(x => x.Descending(u => u.update_last))
                //       .Query(q => q.Bool(bq => bq.Filter(filters)))
                //);
                var search_response_mul = elasticClient.Search<ProductViewModel>(s => s
                                .Index(index_name)
                                     .From(0)
                                     .Size(size)
                                     .Sort(x => x.Descending(u => u.update_last))
                                     .Query(q => q.Term("label_id", (int)LabelType.costco) && q.Term("product_status", (int)ProductStatus.NORMAL_SELL))
                                );
                if (!search_response_mul.IsValid)
                {
                    LogHelper.InsertLogTelegram("1372498309:AAH0fVJfnZQFg5Qaqro47y1o5mIIcwVkR3k", "-309075192", "getProductDetailByProductCodeList grp_id = " + JsonConvert.SerializeObject(product_code_id) + " khong ton tai trong ES");
                }
                else
                {
                    product_result_model = new SearchEsEntitiesViewModel
                    {
                        obj_lst_product_result = search_response_mul.Documents as List<ProductViewModel>,
                        total_item_store = search_response_mul.Total
                    };

                    //LogHelper.InsertLogTelegram("1372498309:AAH0fVJfnZQFg5Qaqro47y1o5mIIcwVkR3k", "-309075192", "error ES getProductDetailByProductCodeList total_item_store = " + product_result_model.total_item_store);

                    if (product_result_model.obj_lst_product_result.Count == 0)
                    {
                        LogHelper.InsertLogTelegram("1372498309:AAH0fVJfnZQFg5Qaqro47y1o5mIIcwVkR3k", "-309075192", "error ES getProductDetailByProductCodeList response_es = " + JsonConvert.SerializeObject(search_response_mul));
                    }
                }


                return product_result_model.total_item_store == 0 ? null : product_result_model;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("1372498309:AAH0fVJfnZQFg5Qaqro47y1o5mIIcwVkR3k", "-309075192", "ex getProductDetailByProductCodeList " + ex.ToString());
                return null;
            }
        }
        public List<ProductViewModel> CostcoModel()
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var search_response = elasticClient.Search<ProductViewModel>(s => s
                    .Index("product")
                    .Query(q => q.Term("label_id", (int)LabelType.costco))
                );
                if (!search_response.IsValid)
                {
                }
                else
                {
                    if (search_response.Documents.Count > 0)
                    {
                        return search_response.Documents as List<ProductViewModel>;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("1264151832:AAFKCM7CoaBerVhcYCHQba1AyY13X41rT5s", "-406438822", "ex getProductDetailByCode " + ex.ToString());
                return null;
            }
        }
        public List<ProductViewModel> GetListProductManual()
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var search_response = elasticClient.Search<ProductViewModel>(s => s
                    .Index("product")
                    .Query(q => q.Term("product_type", (int)ProductType.FIXED_AMOUNT_VND))
                );
                if (!search_response.IsValid)
                {
                }
                else
                {
                    if (search_response.Documents.Count > 0)
                    {
                        return search_response.Documents as List<ProductViewModel>;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("1264151832:AAFKCM7CoaBerVhcYCHQba1AyY13X41rT5s", "-406438822", "ex getProductDetailByCode " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Minh - Xoá tất cả các bản ghi trùng product_code chỉ giữ lại bản ghi mới nhất
        /// </summary>
        /// <param name="index_name"></param>
        /// <param name="product_code"></param>
        /// <param name="label_id"></param>
        /// <returns></returns>
        public int DeleteProductByCodeExceptLastestItem(string index_name, string product_code, int label_id = (int)LabelType.amazon)
        {
            string key_es_id = string.Empty;
            int delete_count = 0;
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                // input
                //var document = new ProductViewModel
                //{
                //    product_code = document_id
                //};
                //// find key
                //var result = elasticClient.Index(document, i => i.Index(index_name));
                /*
                var result = elasticClient.Search<object>(s => s
                                             .Index(index_name)
                                             .Query(q => q
                                             .Term("product_code", document_id))
                                             );*/

                var result = elasticClient.Search<object>(sd => sd
                               .Index(index_name)
                               .Size(4000)
                               .Query(q => q
                                   .Match(m => m.Field("product_code").Query(product_code)
                                   )));
                if (result.IsValid && result.Documents.Count > 0)
                {
                    for (int i = 0; i < (result.HitsMetadata.Hits.Count - 1); i++)
                    {
                        key_es_id = ((Hit<object>)((IHit<object>[])result.HitsMetadata.Hits)[i]).Id;
                        var response = elasticClient.Delete<object>(key_es_id.ToString(), d => d
                        .Index(index_name)
                        );
                        delete_count++;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteProductByCodeExceptLastestItem product_code=" + product_code + " Exception" + ex.ToString());
            }
            return delete_count;

        }
        /// <summary>
        /// Lấy danh sách tất cả sản phẩm có trong group_id
        /// </summary>
        /// <param name="index_name"></param>
        /// <param name="group_id"></param>
        /// <returns></returns>
        public async Task<List<ProductViewModel>> GetListProductCodeByGroupProduct(string index_name, int group_id)
        {
            List<ProductViewModel> result = new List<ProductViewModel>();
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);

                var filters = new List<Func<QueryContainerDescriptor<ProductViewModel>, QueryContainer>>();
                filters.Add(fq => fq.Terms(t => t.Field(f => f.group_product_id).Terms(group_id)));
                var search_response_mul = await elasticClient.SearchAsync<ProductViewModel>(s => s
                       .Index(index_name)
                        .From(0)
                        .Size(1000)
                        .MatchAll()
                       .Sort(x => x.Descending(u => u.update_last))
                       .Query(q => q.Bool(bq => bq.Filter(filters)))
                );
                if (!search_response_mul.IsValid)
                {
                }
                else
                {
                    result = search_response_mul.Documents as List<ProductViewModel>;
                    result = result.GroupBy(x => x.product_code).Select(y => y.First()).ToList();
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("1264151832:AAFKCM7CoaBerVhcYCHQba1AyY13X41rT5s", "-406438822", "ex GetListProductCodeByGroupProduct " + ex.ToString());
            }
            return result;
        }
       
        public int DeleteProductByCodeNew(string index_name, string product_code)
        {
            string key_es_id = string.Empty;
            int delete_count = 0;
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("people");
                var elasticClient = new ElasticClient(connectionSettings);
                var result = elasticClient.Search<object>(sd => sd
                               .Index(index_name)
                               .Size(4000)
                               .Query(q => q
                                   .Match(m => m.Field("product_code").Query(product_code)
                                   )));
                if (result.IsValid && result.Documents.Count > 0)
                {
                    for (int i = 0; i < result.HitsMetadata.Hits.Count; i++)
                    {
                        key_es_id = ((Hit<object>)((IHit<object>[])result.HitsMetadata.Hits)[i]).Id;
                        var response = elasticClient.Delete<object>(key_es_id.ToString(), d => d
                        .Index(index_name)
                        );
                        delete_count++;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteProductByCodeExceptLastestItem product_code=" + product_code + " Exception" + ex.ToString());
            }
            return delete_count;

        }
        
    }




}
