using Pondrop.Service.Store.Domain.Events.ShoppingList;
using Pondrop.Service.Store.Domain.Models;
using System;
using System.Linq;
using Xunit;

namespace Pondrop.Service.Store.Domain.Tests;

public class ShoppingListTests
{
    [Fact]
    public void ShoppingList_NewShoppingList_ShouldCreateShoppingList()
    {
        // arrange
        const string name = "New list!";
        const string description = "My new awesome shopping list";
        var userId = Guid.NewGuid();
        
        // act
        var shoppingList = new ShoppingListEntity(userId, name, description);
        var events = shoppingList.GetEvents().ToList();
        
        // assert
        Assert.NotEqual(Guid.Empty, shoppingList.Id);
        Assert.Equal(userId, shoppingList.UserId);
        Assert.Single(events);
        Assert.Equal(nameof(CreateShoppingList), events.Single().EventPayloadType);
    }
    
    [Fact]
    public void ShoppingList_UpdateShoppingList_ShouldUpdateList()
    {
        // arrange
        const string newName = "My awesome new name";
        const string newDescription = "My awesome new description";
        var shoppingList = GetNewShoppingList();
        var updateEvent = new UpdateShoppingList(shoppingList.Id, newName, newDescription);
        
        // act
        shoppingList.Apply(updateEvent);
        
        // assert
        Assert.Equal(newName, shoppingList.Name);
        Assert.Equal(newDescription, shoppingList.Description);
    }
    
    [Fact]
    public void ShoppingList_AddItemToShoppingList_ShouldAddToList()
    {
        // arrange
        const string itemName = "New item";
        var itemId = Guid.NewGuid();
        var shoppingList = GetNewShoppingList();
        var addToEvent = new AddItemToShoppingList(itemId, shoppingList.Id, itemName);
        
        // act
        shoppingList.Apply(addToEvent);
        
        // assert
        Assert.Single(shoppingList.Items);
        Assert.Equal(itemId, shoppingList.Items.Single().Id);
        Assert.Equal(itemName, shoppingList.Items.Single().Name);
    }
    
    [Fact]
    public void ShoppingList_RemoveItemFromShoppingList_ShouldRemoveFromList()
    {
        // arrange
        const string itemName = "New item";
        var itemId = Guid.NewGuid();
        var shoppingList = GetNewShoppingList();
        var addToEvent = new AddItemToShoppingList(itemId, shoppingList.Id, itemName);
        var removeFromEvent = new RemoveItemFromShoppingList(itemId, shoppingList.Id);
        shoppingList.Apply(addToEvent);
        
        // act
        shoppingList.Apply(removeFromEvent);
        
        // assert
        Assert.Empty(shoppingList.Items);
    }

    private ShoppingListEntity GetNewShoppingList()
    {
        var stringArr = Enumerable.Range(1, 20).Select(i => ($"List {i}", $"Description {i}")).ToArray();
        var idx = new Random().NextInt64() % 20;
        return new ShoppingListEntity(Guid.NewGuid(), stringArr[idx].Item1, stringArr[idx].Item2);
    }
}