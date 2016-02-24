﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmazonBestSellers
{
    public class Domain
    {
        public string RootCategoryName { get; set; }
        public string URL { get; set; }
        public List<Category> Categories { get; set; }

        public Domain(string url, string rootCategoryName = "Books")
        {
            URL = url;
            RootCategoryName = rootCategoryName;
            Categories = new List<Category>();
        }

        public async Task ProcessCategory()
        {
            try
            {
                Category rootCategory = new Category(RootCategoryName, URL);

                List<Task<IEnumerable<Category>>> downloadTasks = new List<Task<IEnumerable<Category>>>();

                for(int page = 1; page <= 5; page++)
                {
                    if(page == 1)
                    {
                        downloadTasks.Add(rootCategory.RetrieveCategoryData(page));
                    }
                    else
                    {
                        downloadTasks.Add(rootCategory.RetrieveCategoryData(page, 0));
                        downloadTasks.Add(rootCategory.RetrieveCategoryData(page, 1));
                    }
                }
                Categories.Add(rootCategory);

                while(downloadTasks.Count > 0)
                {
                    Task<IEnumerable<Category>> firstFinishedTask = await Task.WhenAny(downloadTasks);

                    downloadTasks.Remove(firstFinishedTask);

                    var result = firstFinishedTask.Result;
                    firstFinishedTask.Dispose();

                    if(result != null)
                    {
                        var subCategories = result.ToList();

                        for (int page = 1; page <= 5; page++)
                        {
                            foreach (Category category in subCategories)
                            {
                                if (page == 1)
                                {
                                    downloadTasks.Add(category.RetrieveCategoryData(page));
                                }
                                else
                                {
                                    downloadTasks.Add(category.RetrieveCategoryData(page, 0));
                                    downloadTasks.Add(category.RetrieveCategoryData(page, 1));
                                }
                            }
                        }
                        Categories.AddRange(subCategories);
                    }
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {
                throw ex;
            }
            catch(Exception ex)
            {
                Logger.Log(string.Format("Error retrieving categories for {0}", RootCategoryName), ex);
            }
        }
    }
}
