﻿import BaseClass from "../../assets/models/base-class";
import IView from "../../assets/interfaces/view-interface";

export default class Album extends BaseClass implements IView {
    constructor() {
        super();
    }

    loadView(): void {
        throw new Error("Method not implemented.");
    }
}