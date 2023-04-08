using rainyroute.Models.Data;
using Raven.Client.Documents;

public class Test
{
    IDocumentStore Store;

    public Test(IDocumentStore store)
    {
        Store = store;
    }

    public void GenerateSampleData(){



        using (var session = Store.OpenSession())
        {
            
        }
    }

    public List<WeatherRouteBoundingBox> GenerateGermanySubBoxes(){
        
    }
}