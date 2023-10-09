namespace ISADotNet.Json.ROCrateContext

module Comment =

  let context =
    """
{
  "@context": {
    "sdo": "http://schema.org/",
    "arc": "http://purl.org/nfdi4plants/ontology/",
    
    "Comment": "sdo:Comment",
    "name": "sdo:name",
    "value": "sdo:value"
  }
}
    """