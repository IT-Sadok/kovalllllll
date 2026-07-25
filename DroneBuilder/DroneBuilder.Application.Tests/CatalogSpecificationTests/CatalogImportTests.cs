using DroneBuilder.Application.Features.Catalog.Imports.Models;
using DroneBuilder.Application.Features.Catalog.Imports.ReplacePropertyAliases;
using DroneBuilder.Application.Features.Catalog.Imports.UpsertProductExternalReference;
using DroneBuilder.Application.Repositories;
using DroneBuilder.Application.ResultErrors;
using DroneBuilder.Domain.Entities;
using FluentResults;
using NSubstitute;

namespace DroneBuilder.Application.Tests.CatalogSpecificationTests;

public class CatalogImportTests
{
    private readonly ICatalogImportRepository _repository = Substitute.For<ICatalogImportRepository>();

    [Fact]
    public async Task ReplacePropertyAliases_PreservesGlobalAndOtherSourceAliases()
    {
        var source = new ImportSource { Code = "vendor-a", Name = "Vendor A" };
        Guid otherSourceId = Guid.NewGuid();
        var property = new Property { Code = "motor-kv", Name = "Motor KV" };
        property.Aliases.Add(new PropertyAlias { PropertyId = property.Id, Alias = "KV", ImportSourceId = null });
        property.Aliases.Add(new PropertyAlias
        {
            PropertyId = property.Id,
            Alias = "Old vendor alias",
            ImportSourceId = source.Id
        });
        property.Aliases.Add(new PropertyAlias
        {
            PropertyId = property.Id,
            Alias = "Other source alias",
            ImportSourceId = otherSourceId
        });
        _repository.GetSourceAsync(source.Id, Arg.Any<CancellationToken>()).Returns(source);
        _repository.GetPropertyAsync(property.Id, Arg.Any<CancellationToken>()).Returns(property);

        Result<ICollection<string>> result = await new ReplacePropertyAliasesCommandHandler(_repository)
            .ExecuteCommandAsync(
                new ReplacePropertyAliasesCommand(
                    source.Id,
                    property.Id,
                    new ReplaceSourceAliasesModel { Aliases = ["Motor KV", " motor   kv "] }),
                CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Contains(property.Aliases, alias => alias.ImportSourceId is null && alias.Alias == "KV");
        Assert.Contains(property.Aliases, alias => alias.ImportSourceId == otherSourceId);
        Assert.Contains(property.Aliases, alias => alias.ImportSourceId == source.Id && alias.Alias == "Motor KV");
        Assert.DoesNotContain(property.Aliases, alias => alias.Alias == "Old vendor alias");
    }

    [Fact]
    public async Task UpsertProductReference_WhenNew_AddsReference()
    {
        var source = new ImportSource { Code = "vendor", Name = "Vendor" };
        var product = new Product { Name = "Motor" };
        _repository.GetSourceAsync(source.Id, Arg.Any<CancellationToken>()).Returns(source);
        _repository.GetProductAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _repository.GetProductReferenceAsync(source.Id, "SKU-1", Arg.Any<CancellationToken>())
            .Returns((ProductExternalReference?)null);
        var model = new UpsertExternalReferenceModel
        {
            ExternalId = "SKU-1",
            SourceUrl = "https://vendor.test/items/1",
            ContentHash = "hash"
        };

        Result<ExternalReferenceModel> result =
            await new UpsertProductExternalReferenceCommandHandler(_repository)
                .ExecuteCommandAsync(
                    new UpsertProductExternalReferenceCommand(product.Id, source.Id, model),
                    CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _repository.Received(1).AddProductReferenceAsync(
            Arg.Is<ProductExternalReference>(reference =>
                reference.ProductId == product.Id && reference.ExternalId == "SKU-1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpsertProductReference_WhenExternalIdBelongsToAnotherProduct_ReturnsConflict()
    {
        var source = new ImportSource { Code = "vendor", Name = "Vendor" };
        var product = new Product { Name = "Motor" };
        var existing = new ProductExternalReference
        {
            ImportSourceId = source.Id,
            ProductId = Guid.NewGuid(),
            ExternalId = "SKU-1"
        };
        _repository.GetSourceAsync(source.Id, Arg.Any<CancellationToken>()).Returns(source);
        _repository.GetProductAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _repository.GetProductReferenceAsync(source.Id, "SKU-1", Arg.Any<CancellationToken>())
            .Returns(existing);

        Result<ExternalReferenceModel> result =
            await new UpsertProductExternalReferenceCommandHandler(_repository)
                .ExecuteCommandAsync(
                    new UpsertProductExternalReferenceCommand(
                        product.Id,
                        source.Id,
                        new UpsertExternalReferenceModel { ExternalId = "SKU-1" }),
                    CancellationToken.None);

        Assert.True(result.HasError<ConflictError>());
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
