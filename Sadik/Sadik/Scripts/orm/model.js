Math.guid = function(){
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
    var r = Math.random()*16|0, v = c == 'x' ? r : (r&0x3|0x8);
    return v.toString(16);
  }).toUpperCase();      
};

Math.defaultGuid = '00000000-0000-0000-0000-000000000000';

var Model = Klass.create();

// Alias create
Model.createSub = Model.create;
Model.setup = function(name, atts){
  var model = this.createSub();
  if (name) model.name = name;
  if (atts) model.attributes = atts;
  return model;
};

Model.extend({
 init: function(){
   this.records = {};
   this.attributes = [];
 },
 
 find: function (UniqueId) {
   var record = this.records[UniqueId.toUpperCase()]
   if (!record) record = this.records[UniqueId.toLowerCase()];
   if ( !record ) throw("Unknown record");
   return record.dup();
 },
 
 exists: function (UniqueId) {
   try {
       return this.find(UniqueId);
   } catch (e) {
     return false;
   }
 },
 
 populate: function(values){
   // Reset model & records
   this.records = {};
   
   for (var i=0, il = values.length; i < il; i++) {    
       var record = this.inst(values[i]);
     record.newRecord = false;
     this.records[record.UniqueId] = record;
   }
 },
 
 merge: function (values) {
     if (!this.records) this.records = {};
     for (var i = 0, il = values.length; i < il; i++) {
         var record = this.records[values[i].UniqueId];
         if (!record || !record.isDirty) {
             var record = this.inst(values[i]);
             record.newRecord = false;
             this.records[record.UniqueId] = record;
         }
     }
 },

 select: function(callback){
   var result = [];
   
   for (var key in this.records)
     if (callback(this.records[key]))
       result.push(this.records[key]);
   
   return this.dupArray(result);
 },
 
 findByAttribute: function(name, value){
   for (var key in this.records)
     if (this.records[key][name] == value)
       return this.records[key].dup();
 },
 
 findAllByAttribute: function(name, value){
   return(this.select(function(item){
     return(item[name] == value);
   }));
 },
 
 each: function(callback){
   for (var key in this.records) {
     callback(this.records[key]);
   }
 },
 
 all: function(){
   return this.dupArray(this.recordsValues());
 },
 
 first: function(){
   var record = this.recordsValues()[0];
   return(record && record.dup());
 },

 last: function(){
   var values = this.recordsValues()
   var record = values[values.length - 1];
   return(record && record.dup());
 },
 
 count: function(){
   return this.recordsValues().length;
 },
 countDirty: function () {
     var counter = 0;
     for (var key in this.records)
         if (this.records[key].isDirty)
             counter++;

     return counter;
 },
 deleteAll: function(){
   for (var key in this.records)
     delete this.records[key];
 },

 destroyAll: function(){
   for (var key in this.records)
     this.records[key].destroy();
 },

 update: function (UniqueId, atts) {
     this.find(UniqueId).updateAttributes(atts);
 },
 
 create: function(atts){
     var record = this.inst(atts);
   record.save();
   return record;
 },

 destroy: function (UniqueId) {
     this.find(UniqueId).destroy();
 },
 
 // Private
 
 recordsValues: function(){
   var result = []
   for (var key in this.records)
     result.push(this.records[key])
   return result;
 },
 
 dupArray: function(array){
   return array.map(function(item){
     return item.dup();
   });
 },

 saveLocal: function (name) {
     // Turn records into an array
     var result = [];
     for (var i in this.records)
         result.push(this.records[i])
     localStorage[name] = JSON.stringify(result);
 },
 saveLocalDirtyOnly:function(name){
     var result = [];
     for (var i in this.records){
         if(this.records[i].isDirty)
             result.push(this.records[i]);
     }
     localStorage[name] = JSON.stringify(result);
 },
 loadLocal: function (name) {
     var result = JSON.parse(localStorage[name]);
     this.populate(result);
 }
});
 
Model.include({
  newRecord: true,
  isDirty: false,

  init: function(atts){
    if (atts) this.load(atts);
  },
  
  isNew: function(){
    return this.newRecord;
  },
  
  validate: function () { },

  load: function(attributes){
    for(var name in attributes)
      this[name] = attributes[name];
  },

  attributes: function(){
    var result = {};
    for(var i in this.parent.attributes) {
      var attr = this.parent.attributes[i];
      result[attr] = this[attr];
    }
    result.UniqueId = this.UniqueId;
    return result;
  },
  
  eql: function(rec){
      return (rec && rec.UniqueId === this.UniqueId &&
           rec.parent === this.parent);
  },

  save: function(){
    if (this.validate() !== true) return false;
    this.publish("beforeSave");
    this.newRecord ? this.create() : this.update();
    this.publish("afterSave");
    this.publish("save");
  },
  
  updateAttribute: function(name, value){
    this[name] = value;
    return this.save();
  },
  
  updateAttributes: function(attributes){
    this.load(attributes);
    return this.save();
  },

  destroy: function(){
    this.publish("beforeDestroy");
    delete this.parent.records[this.UniqueId];
    this.publish("afterDestroy");
    this.publish("destroy");
  },

  dup: function(){
    return Object.create(this);
  },

  toJSON: function(){
    return(this.attributes());
  },
  
  // Private
  
  update: function(){
    this.publish("beforeUpdate");
    this.parent.records[this.UniqueId] = this.dup();
    this.isDirty = true;
    this.publish("afterUpdate");
    this.publish("update");
  },
  
  generateID: function(){
    return Math.guid();
  },
  
  create: function(){
    this.publish("beforeCreate");
    if (!this.UniqueId) this.UniqueId = this.generateID();
    this.newRecord = false;
    this.parent.records[this.UniqueId] = this.dup();
    this.isDirty = true;
    this.publish("afterCreate");
    this.publish("create");
  },
  
  publish: function(channel){
    this.parent.publish(channel, this);
  },

  createRemote: function (url, callback) {
      var self = this;
      this.publish("beforeCreateRemote");
      this.publish("beforeSaveRemote");
      $.ajax({
          url: url,
          data: this.attributes(),
          success: function () {
              self.isDirty = false;
              self.publish("afterCreateRemote");
              self.publish("afterSaveRemote");
              if (callback && typeof callback == 'function') {
                  callback.apply(self, arguments);
              }
          },
          dataType: 'json',
          type: "POST"
      });
  },

  updateRemote: function (url, callback) {
      var self = this;
      this.publish("beforeUpdateRemote");
      this.publish("beforeSaveRemote");
      $.ajax({
          url: url,
          data: this.attributes(),
          success: function () {
              self.isDirty = false;
              self.publish("afterUpdateRemote");
              self.publish("afterSaveRemote");
              if (callback && typeof callback == 'function') {
                  callback.apply(self, arguments);
              }
          },
          dataType: 'json',
          type: "PUT"
      });
  },
  destroyRemote: function (url, callback) {
      var self = this;
      this.publish("beforeDestroyRemote");
      this.publish("beforeSaveRemote");
      console.log(self.UniqueId);
      $.ajax({
          url: url,
          data: { UniqueId: self.UniqueId },
          success: function () {
              self.isDirty = false;
              self.publish("afterDestroyRemote");
              self.publish("afterSaveRemote");
              if (callback && typeof callback == 'function') {
                  callback.apply(self, arguments);
              }
          },
          dataType: 'json',
          type: "DELETE"
      });
  }
});

Model.extend(PubSub);