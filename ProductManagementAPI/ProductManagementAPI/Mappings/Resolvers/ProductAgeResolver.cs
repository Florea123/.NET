using AutoMapper;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;
using System;

namespace ProductManagementAPI.Mapping.Resolvers;

public class ProductAgeResolver : IValueResolver<Product, ProductProfileDto, string>
{
    public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
    {
        var created = source.CreatedAt;
        var now = DateTime.UtcNow;
        if (created > now) created = now;
        var totalDays = (now - created).TotalDays;

        if (totalDays < 30) return "New Release";
        if (totalDays < 365)
        {
            var months = (int)Math.Floor(totalDays / 30);
            return $"{months} month{(months > 1 ? "s" : "")} old";
        }
        
        if ((int)totalDays == 1825) return "Classic";

        if (totalDays < 1825)
        {
            var years = (int)Math.Floor(totalDays / 365);
            return $"{years} year{(years > 1 ? "s" : "")} old";
        }
        
        var yearsBeyond = (int)Math.Floor(totalDays / 365);
        return $"{yearsBeyond} year{(yearsBeyond > 1 ? "s" : "")} old";
    }
}