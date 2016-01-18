# AzureBlobSearchHelper

What?
-----
Generic helper class for uploading files and meta data to blob storage and to provision and manage a search service over those blobs and meta data 

How?
----

If you have a

```c#
public class Foo{
    public int Id { get; set;}
    public string Description {get; set;}
}

```

you start by decorating with [MetaName] and [Meta] attributes

```c#
public class Foo{
    [MetaName]
    public int Id { get; set;}
    [Meta]
    public string Description { get; set;}
}

```

you'll need 1 [MetaName] and 0+ [Meta] decorated properties.

Create an AzureFileStorage object passing in an Azure Container and func to get the file bytes 

```c#
   var fs = new AzureFileStorage<Foo>(myAzureContainer,f => GetMyFile(f.Id))

```
 Then use TrySaveItemAsync and GetMetaItemAsync to save and fetch from the storage