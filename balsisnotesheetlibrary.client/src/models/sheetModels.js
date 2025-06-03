export class NoteSheet {
  constructor(props = {}) {
    Object.assign(this, props);
  }

  /** @type {Number} */
  id;
  /** @type {string} */
  title;
  /** @type {?string} */
  author;
  /** @type {?string} */
  lyricist;
  /** @type {?Number} */
  year;
  /** @type {string} */
  filename;
  /** @type {boolean} */
  isLatvian;

  getFormattedAdditionalData() {
    const dataParts = [this.author, this.lyricist, this.year].filter(Boolean);

    return dataParts.length > 0 ? ", " + dataParts.join(", ") : "";
  }
}

export class SetList {
  constructor(props = {}) {
    Object.assign(this, props);

    this.items = props.items ? props.items.map((item) => new SetListItem(item)) : [];
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
    const noteSheetIds = this.items.map((item) => item.noteSheetId);

    return allNoteSheets.filter((noteSheet) =>
      noteSheetIds.includes(noteSheet.id),
    );
  }
}

export class SetListItem {
  constructor(props = {}) {
    Object.assign(this, props);
  }

  /** @type {Number} */
  id;
  /** @type {Number} */
  noteSheetId;
  /** @type {string} */
  setListId;
  /** @type {Number} */
  order;
}
