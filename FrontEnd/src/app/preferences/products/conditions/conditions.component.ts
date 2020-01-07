import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';
import { PrinterSetupModalComponent } from "../../printer-setup/printer-setup-modal/printer-setup-modal.component";
import { BsModalService } from "ngx-bootstrap";
import { ConditionModalComponent } from "./condition-modal/condition-modal.component";

@Component({
  selector: 'app-conditions',
  templateUrl: './conditions.component.html',
  styleUrls: ['./conditions.component.css']
})
export class ConditionsComponent implements OnInit {
  conditions: any[];
  loading = false;
  syncing: boolean = false;
  lastSynced;

  constructor(private api: ApiService, private modalService: BsModalService) { }

  ngOnInit() {
    this.getConditions();
    this.GetLastSynced();
  }

  getConditions() {
    this.loading = true;
    this.api.GetConditions({},
      data => {
        this.conditions = data;
        this.loading = false;
      },
      error => {
        this.loading = false;
        console.log(error);
      } 
    );
  }

  setConvertible(condition) {
    this.api.UpdateConditions(
      [condition],
      data => {},
      error => {

        condition.Convertible = !condition.Convertible;
        
        if(error.Messages) {
          alert(error.Messages.join(', '));
        } else {
          alert('Error Saving');
        }
      }
    );
  }

  Sync(){
    this.syncing = true;
    this.api.Sync(1, //channel id goes here
      data => {
      this.getConditions();
      this.GetLastSynced();
      this.syncing = false;

    }, error => {
      alert("there was an error while trying to sync");
      this.syncing = false;
      console.log(error);
    })
  }

  GetLastSynced() {
    this.api.GetLastSync("SyncProducts",
      data => {
        this.lastSynced = data;
      },
      error => console.log(error)
    );
  }

  openEditConditionPopup(condition) {
    const initialState = {
      condition: {...condition},
    };

    const bsModalRef = this.modalService.show(ConditionModalComponent, {initialState});
    bsModalRef.content.onClose.subscribe(result => {
      if (result.success) {
        this.getConditions();
      }
    });
  }

}
