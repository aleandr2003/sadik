function SearchDictionary(dictionary, comparer) {
    for (var i in dictionary) {
        item = dictionary[i];
        if (comparer(item)) {
            return item;
        }
    }
    return null;
}