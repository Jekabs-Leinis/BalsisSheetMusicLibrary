export class NoteSheet {
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
  filename= "";
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

  /**
   * @param allNoteSheets {NoteSheet[]}
   * @returns {NoteSheet[]}
   */
  getNoteSheets(allNoteSheets) {
    const noteSheetIds = new Set(this.items.map((item) => item.noteSheetId));

    return allNoteSheets.filter((noteSheet) =>
      noteSheetIds.has(noteSheet.id),
    );
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
  }
  
  /** @type {Number} */
  noteSheetId;
  /** @type {string} */
  setListId;
  /** @type {Number} */
  order;
}
