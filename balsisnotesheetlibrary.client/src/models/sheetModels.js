export class NoteSheet {
  constructor(props = {}) {
    Object.assign(this, props);
  }

  /** @type {string} */
  id;
  /** @type {string} */
  title;
  /** @type {?string} */
  author;
  /** @type {?string} */
  lyricist;
  /** @type {?number} */
  year;
  /** @type {string} */
  downloadLink;

  getFormattedAdditionalData() {
    const dataParts = [this.author, this.lyricist, this.year].filter(Boolean);

    return dataParts.length > 0 ? ", " + dataParts.join(", ") : "";
  }
}

export class SetList {
  constructor(props = {}) {
    Object.assign(this, props);
  }

  /** @type {string[]} */
  ids = [];
  /** @type {string} */
  title;

  /**
   * @param allNoteSheets {NoteSheet[]}
   * @returns {NoteSheet[]}
   */
  getNoteSheets(allNoteSheets) {
    return allNoteSheets.filter((noteSheet) => this.ids.includes(noteSheet.id));
  }
}
