﻿using ArticlesAPI.Data;
using ArticlesAPI.Entities;
using ArticlesAPI.Models;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;

namespace ArticlesAPI.Services
{
    public class ArticleService : IArticleService
    {
        //przekazywanie contextu bazy danych do serwisu
        private readonly ArticlesContext _context; 

        public ArticleService(ArticlesContext context)
        {
            _context = context;
        }

        //dodawanie
        public async Task<bool> AddArticle(AddArticleDto article)
        {
            var newArticle = new Article();
            newArticle.Title = article.Title;
            newArticle.Description = article.Description;
            newArticle.ImageURL = article.File.FileName;

            //dodawanie image do storage accounta
            var blobStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=psarticle;AccountKey=5vkxEfcI2YJEWi9HrIeP+lJZCOA48uEWz1eV9/bRPfASocHHfbcyb7h/hcLN6cCVfw9rI8AKm+xU+AStmyFYCg==;EndpointSuffix=core.windows.net";
            var blobStorageContainername = "files";

            var container = new BlobContainerClient(blobStorageConnectionString, blobStorageContainername);
            var blob = container.GetBlobClient(article.File.FileName);
            
            var stream = article.File.OpenReadStream();
            await blob.UploadAsync(stream);

            _context.Articles.Add(newArticle);
            await _context.SaveChangesAsync();
            return true;
        }

        //usuwanie
        public async Task<bool> DeleteArticle(string articleTitle)
        {
            if (_context.Articles == null)
            {
                throw new Exception("Baza danych jest pusta");
            }

            var article = _context.Articles.FirstOrDefault(x => x.Title == articleTitle);

            if (article == null)
            {
                return false;
            }

            _context.Articles.Remove(article);

            if (await _context.SaveChangesAsync() == 1)
            {
                return true;
            }
            return false;
        }

        //edytowanie
        public async Task<bool> EditArticle(string articleTitle, EditArticleDtoIn article)
        {
            var _article = _context.Articles.FirstOrDefault(x => x.Title == articleTitle);

            if (_article == null)
            {
                throw new Exception("Nie ma takiego artykułu");
            }
            
            _article.Title = article.Title;
            _article.Description = article.Description;
            _article.ImageURL = article.ImageURL;   

            if (await _context.SaveChangesAsync() == 1)
            {
                return true;
            }

            throw new Exception("Nie dziala");

        }

        //wyswietlanie wszystkich
        public async Task<List<GetAllArticlesDto>> AllArticles()
        {
            return await _context.Articles.Select(x => new GetAllArticlesDto
            {
                Title = x.Title,
                Description = x.Description,
                ImageURL = x.ImageURL
            }).ToListAsync();
        }

        public async Task<GetSingleArticleDto> SingleArticle(string title)
        {
            if (_context.Articles == null)
            {
                throw new Exception("Baza danych jest pusta");
            }

            var _article = _context.Articles.FirstOrDefault(x => x.Title == title);
            
            if (_article == null)
            {
                throw new Exception("Cos zawiodlo");
            }

            var _articleToShowOff = new GetSingleArticleDto();
            _articleToShowOff.Title = _article.Title;
            _articleToShowOff.Description = _article.Description;
            _articleToShowOff.ImageURL = _article.ImageURL;
            return _articleToShowOff;
        }
    }
}
