/* List Blocks and their Attributes*/
SELECT
    [p].[InternalName] [Page.InternalName],    
    [b].[Name] [Block.Name],
    [bt].[Name] [BlockType.Name],
    cast( 
        (select 
            a.Name
            ,av.Value
            ,a.Guid [Attribute.Guid] 
            ,av.Guid [AttributeValue.Guid] 
        from 
            AttributeValue av 
        join 
            Attribute a on av.AttributeId = a.Id 
        join 
            EntityType et on a.EntityTypeId = et.Id
        where 
            et.Name = 'Rock.Model.Block' 
        and 
            av.EntityId = b.Id
        FOR XML PATH ('Attribute'), root ('root' )) as XML) [AttributeValues],
    b.Guid [Block.Guid],
    bt.Guid [BlockType.Guid]
FROM            
   [Block] [b]
    join [BlockType] bt on b.BlockTypeId = bt.Id
    join [Page] p on b.PageId = p.Id
order by p.InternalName, b.Name, bt.Name

  

