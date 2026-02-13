export class SheetMusic {
  constructor(props = {}) {
    Object.assign(this, props);
  }

  /** @type {Number} */
  id;
  /** @type {string} */
  title= "";
  /** @type {?string} */
  author= "";
  /** @type {?string} */
  lyricist= "";
  /** @type {?Number} */
  year= null;
  /** @type {string} */
  fileName= "";
  /** @type {boolean} */
  isLatvian= false;

  getFormattedAdditionalData() {
    const dataParts = [this.author, this.lyricist, this.year].filter(Boolean);

    return dataParts.length > 0 ? ", " + dataParts.join(", ") : "";
  }
  
  getFormattedTitle() {
    return this.title + this.getFormattedAdditionalData();
  }
}

export class SetList {
  constructor(props = {}) {
    Object.assign(this, props);

    this.items = props.items ? props.items.map((item) => new SetListItem(item)) : [];
    
    if (props.createdAt) {
      this.createdAt = new Date(props.createdAt);
    }
    
    if (props.archivedAt) {
      this.archivedAt = new Date(props.archivedAt);
    }
    
    // Items are retrieved by insertion order, so we need to sort them by order.
    this.sortItems();
    // In some cases, the order might have a gap, so we regenerate order values.
    this.reorderItems();
  }

  /** @type {Number} */
  id;
  /** @type {SetListItem[]} */
  items = [];
  /** @type {string} */
  title;
  /** @type {Number} */
  order;
  /** @type {Date} */
  createdAt;
  /** @type {Date} */
  archivedAt;

  /**
   * @param allSheetMusic {SheetMusic[]}
   * @returns {SheetMusic[]}
   */
  getSheetMusic(allSheetMusic) {
    // Create a map for quick lookup of SheetMusic by id
    const sheetMusicMap = new Map(allSheetMusic.map(ns => [ns.id, ns]));
    
    // Return SheetMusic in the order of this.items by their order property
    return this.items
      .slice() // avoid mutating original array
      .sort((a, b) => a.order - b.order)
      .map(item => sheetMusicMap.get(item.sheetMusicId))
      .filter(Boolean);
  }
  
  reorderItems() {
    this.items.forEach((item, index) => item.order = index);
    this.sortItems();
  }
  
  sortItems() {
    this.items.sort((a, b) => a.order - b.order);
  }
}

export class SetListItem {
  constructor(props = {}) {
    Object.assign(this, props);
    
    if (props.sheetMusic) {
      this.sheetMusic = new SheetMusic(props.sheetMusic);
    }
  }
  
  /** @type {Number} */
  sheetMusicId;
  /** @type {string} */
  setListId;
  /** @type {Number} */
  order;
  /** @type {SheetMusic} */
  sheetMusic;
}
