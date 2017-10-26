<h1># Service Fabric PartitionService</h1>

Sample project based on Microsoft Service
Fabric and concept from <a href="https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-concepts-partitioning">Partition Service Fabric reliable services</a>

<p>More info on Service Fabric:</p>

<ul>
<li><a href="https://docs.microsoft.com/azure/service-fabric/">Documentation</a></li>
<li><a href="https://azure.microsoft.com/resources/samples/?service=service-fabric">More sample projects</a></li>
<li><a href="https://github.com/azure/service-fabric">Service Fabric open source home repo</a></li>
</ul>

<h1>How to run</h1>

1. Open solution in VS 2017.
2. Run Debug or deploy to local cluster.
3. Open browser and go to http://localhost:8081/alphabetpartitions?lastname={your_data}
Where <i>{your_data}</i> is sample name, e.g.:  http://localhost:8081/alphabetpartitions?lastname=testuser