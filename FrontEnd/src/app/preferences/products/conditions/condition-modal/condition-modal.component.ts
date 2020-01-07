import { Component, OnInit } from '@angular/core';
import { Subject } from "rxjs";
import { BsModalRef } from "ngx-bootstrap";
import { ApiService } from "../../../../services/api.service";

@Component({
  selector: 'app-condition-modal',
  templateUrl: './condition-modal.component.html',
  styleUrls: ['./condition-modal.component.css']
})
export class ConditionModalComponent implements OnInit {

  condition: any = {};
  onClose: Subject<any>;

  constructor(public bsModalRef: BsModalRef, private api: ApiService) {
  }

  ngOnInit() {
    this.onClose = new Subject();
  }

  save() {
    this.api.UpdateConditions([this.condition], data => {
        this.close(true);
      }, error => {
        alert(error.Messages.join(','));
      }
    );
  }

  close(success: boolean) {
    this.onClose.next({
      success: success
    });
    this.bsModalRef.hide();
  }

}
