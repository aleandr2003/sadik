function SearchDictionary(dictionary, comparer) {
    for (var i in dictionary) {
        item = dictionary[i];
        if (comparer(item)) {
            return item;
        }
    }
    return null;
};
function Where(collection, callback) {
    var result = [];
   
    for (var key in collection)
        if (callback(collection[key]))
            result.push(collection[key]);
   
    return result;
};