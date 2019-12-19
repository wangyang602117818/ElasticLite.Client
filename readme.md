**轻量版操作ElasticSearch工具**

>   **Nuget 包搜索 ElasticLite.Client 并且安装**

>   **实例化**

List\<string\> urls = new List\<string\>(){ "http://localhost:9200/"};

ElasticConnection elasticConnection = new ElasticConnection(urls);

1.  判断 index 是否存在:

    bool exists = elasticConnection.Head(indexName);

2.  创建 index, 并且设置mapping:

    var result = elasticConnection.Put(indexName, mapping);

    if (result.Contains("\\"acknowledged\\":true")) return true;

3.  索引数据:

    var result = elasticConnection.Post("person/doc/1", json);

    if (result.Contains("\\"successful\\":1")) return true;

4.  删除数据:

    var result = elasticConnection.Delete("person/doc/1");

>   if (result.Contains("\\"successful\\":1")) return true;

1.  搜索数据:

    var result = elasticConnection.Post(indexName, json);

    或者只取需要的数据

    var result = elasticConnection.Post(indexName+
    "/_search?filter_path=hits.total,hits.hits._source,hits.hits._id,hits.hits.highlight",
    json);

2.  更新数据最佳实践:

    确保id一致使用索引接口来修改
