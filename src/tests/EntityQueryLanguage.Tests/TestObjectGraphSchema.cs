using System.Linq;

// This is a mock datamodel, what would be your real datamodel and EF context
namespace EntityQueryLanguage.Tests
{
    // this is a schema that maps back to your current data model, helping you version APIs. You can change your current data model
    // and keep the API valid by continuing to return the expected objects and fields.
    //
    // The key here is that when you change the underlying data model and entities you get a compile error, fixing them to return what is expected
    // of these classes means you can make non-breaking changes to your exposed API
    namespace ApiVersion1
    {
        internal class TestObjectGraphSchema : MappedSchemaProvider<TestDataContext>
        {
            public TestObjectGraphSchema()
            {
                // we define each type we and their fields

                // Without the fields argument we expose Location fields as-is. Easy and simple, but this means changes in
                // Location data model may break API
                var location = AddType<Location>(name: "location", description: "A geographical location");
                location.AddAllFields();

                // It's better to define the fields of each type you want to expose, so over time your data model can change and
                // if you keep these definitions compiling you shouldn't break any API calls
                // leaving out name will take the name of the base class type
                var person = AddType<Person>("Details of a person in the system");
                // you don't need to define the return type unless you need to specify the type to map to
                person.AddField(p => p.Id, "The unique identifier");
                person.AddField(p => p.Guid, "A Guid identifier");
                person.AddField(p => p.Name, "Person's first name");
                person.AddField(p => p.LastName, "Person's last name");
                // You can provide a name for the field and in composite fields you are required to
                person.AddField("fullName", p => p.Name + " " + p.LastName, "Person's full name");

                var project = AddType<Project>("project", "Details of a project");
                project.AddField(p => p.Id, "Unique identifier for the project");
                project.AddField("name", p => p.Owner.Name + "'s Project", "Project's name");

                // Returning a Location type object will automatically map to the defined location schema above as it is the only one
                project.AddField(p => p.Location, "The location of the project");

                // If you have a Type used multiple times in the schema you will need to define the return by name
                project.AddField("openTasks", p => p.Tasks.Where(t => t.IsActive), "All open tasks for the project", "openTask");
                project.AddField("closedTasks", p => p.Tasks.Where(t => !t.IsActive), "All closed tasks for the project", "closedTask");

                // You can define multiple types from one base type and define a filter which is applied - a poor example
                // any time an `openTask` type is requested the filter will also be applied
                var openTasks = AddType<Task>("openTask", "Details of a project", t => t.IsActive);
                openTasks.AddField(t => t.Id, "Unique identifier for a task");
                openTasks.AddField(t => t.Name, "Description of the task");
                openTasks.AddField(t => t.Assignee, "Active person on the task");

                var closedTasks = AddType<Task>("closedTask", "Details of a project", t => !t.IsActive);
                closedTasks.AddField(t => t.Id, "Unique identifier for a task");
                closedTasks.AddField(t => t.Name, "Description of the task");

                // Now we defined what fields are at the root of the graph

                // Name is inferred. Schema type is inferred
                AddField(ctx => ctx.Locations, "All locations in the world");
                // name is inferred. schema type is inferred by the Type contained in People<Person>
                AddField(ctx => ctx.People, "All our people");
                // name required - creating a filtered view. Schema type required
                AddField("publicProjects", ctx => ctx.Projects.Where(p => p.Type == 2), "All projects marked as public");
                AddField("privateProjects", ctx => ctx.Projects.Where(p => p.Type == 1), "All privately held projects");
                // providing the schema type `openTask` will automatically apply the filter
                AddField("openTasks", ctx => ctx.Tasks, "All open tasks for all projects", "openTask");
                AddField("closedTasks", ctx => ctx.Tasks, "All closedtasks for all projects", "closedTask");
                AddField("defaultLocation", ctx => ctx.Locations.First(l => l.Id == 10), "The default location for projects");

                // add a call with arguments - TODO
                // AddField("person", new { Id = 0 }, (ctx, args) => ctx.People.Where(p => p.Id == args.Id).FirstOrDefault(), "Get a single person by their ID");
            }
        }
    }
}