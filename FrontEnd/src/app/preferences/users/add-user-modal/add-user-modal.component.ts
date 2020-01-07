import { Component, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap';
import { ApiService } from 'src/app/services/api.service';
import { Subject } from 'rxjs';

const printerMock1 = {
  IsAssigned: true,
  title: 'some new printer',
  info: 'some info'
};

const printerMock2 = {
  IsAssigned: false,
  title: 'some new printer2',
  info: 'some info2'
};

@Component({
  selector: 'app-add-user-modal',
  templateUrl: './add-user-modal.component.html',
  styleUrls: ['./add-user-modal.component.css']
})
export class AddUserModalComponent implements OnInit {

  onClose: Subject<any>;
  user: any = {
    Roles: [],
    Printers: []
  };
  Roles = [];
  printersListShown = false;
  isAllPrintersChecked = false;
  hasAnyPrinter: boolean = false;
  selectedTab = "user";
  activeConvert = "locations";

  constructor(public bsModalRef: BsModalRef, private api: ApiService) {
  }

  ngOnInit() {
    this.onClose = new Subject();
    this.getSublocations();
    this.getConditions();
    this.getPrinters();
    this.Roles = this.user.Roles.slice();
  }

  getSublocations() {
    let $this = this;
    this.api.GetSublocations({
      UserId: this.user.UserId,
      Status: true,
      OnlySublocation: true
    }, 1, //channel id here
      data => {

        this.user.Sublocations = data.filter(x => x.ConvertibleFrom == true || x.ConvertibleTo == true);
      },
      error => console.log(error)
    );
  }

  getConditions() {
    let $this = this;
    this.api.GetConditions({
      UserId: this.user.UserId,
      Status: true
    },
      data => {
        this.user.Conditions = data.filter(x => x.Convertible == true);
      },
      error => console.log(error)
    );
  }

  getPrinters() {
    let $this = this;
    this.api.GetPrinters({
      UserId: this.user.UserId,
      Status: true
    },
      data => {
        this.user.Printers = data.List;
        $this.isAllPrintersChecked = this.user.Printers.every((p) => p.IsAssigned);
        $this.hasAnyPrinter = $this.user.Printers.filter(p => p.IsAssigned).length > 0;
      },
      error => console.log(error)
    );
  }

  printerSelected() {
    this.isAllPrintersChecked = this.user.Printers.every((p) => p.IsAssigned);
  }

  setUserRole(role) {
    if(this.Roles.includes(role)) {
      this.Roles = this.Roles.filter(r => r != role);

      if(role == 'Converter' && this.Roles.includes('ConvertPlus')) {
        this.Roles = this.Roles.filter(r => r != 'ConvertPlus');
      }
    } else {
      this.Roles.push(role);

      if(role == 'ConvertPlus' && !this.Roles.includes('Converter')) {
        this.Roles.push('Converter');
      }
    }

    if(role == 'Admin') {
      this.user.IsAdmin = !this.user.IsAdmin;
    }
  }

  removerUserRoles(roles) {
    this.Roles = this.Roles.filter(r => !roles.includes(r));
    
    if(roles.includes('Admin')) {
      this.user.IsAdmin = false;
    }
  }

  save(user) {
    
    this.savePrinters();
    this.user.Roles = this.Roles;

    if (user.Initials == null) {
      this.GetIntials(user)
    }

    if (user.UserId) {

      //update
      this.api.UpdateUser(user,
        data => {
          this.close(true);
        },
        error => {
          alert(error.Messages.join(','));
        }
      );

      this.saveLocations();
      this.saveConditions();

    } else {

      //insert
      user.Active = true;
      this.api.AddUser(user,
        data => {
          console.log(data);
          this.user.UserId = data.Id;
          this.saveLocations();
          this.saveConditions();
          this.close(true);
        },
        error => {
          alert(error.Messages.join(','));
        }
      );
    }
  }


  GetIntials(user) {
    var Nam = user.FirstName + " " + user.LastName;
    var names = Nam.split(" ");
    var initials = names[0].substring(0, 1).toUpperCase();

    if (names.length > 1) {
      initials += names[names.length - 1].substring(0, 1).toUpperCase();
      user.Initials = initials;
    }

  };

  showPrintersList() {
    this.printersListShown = true;
  }

  close(success: boolean) {
    this.onClose.next({
      success: success
    });
    this.bsModalRef.hide();
  }

  saveLocations() {
    this.api.UpdateUserLocations(this.user.UserId, this.user.Sublocations, 1, // channel id here
      data => {

      },
      error => {
        alert(error.Messages.join(','));
      }
    )
  }

  saveConditions() {
    this.api.UpdateUserConditions(this.user.UserId, this.user.Conditions,
      data => {

      },
      error => {
        alert(error.Messages.join(','));
      }
    )
  }

  savePrinters() {
    this.hasAnyPrinter = this.user.Printers.filter(p => p.IsAssigned).length > 0;
    if(this.hasAnyPrinter && !this.Roles.includes("Printer")) {
      this.Roles.push("Printer");
    } else if(!this.hasAnyPrinter && this.Roles.includes("Printer")) {
      this.Roles = this.Roles.filter(role => role != "Printer")
    }
    this.printersListShown = false;
  }

  selectAll(status: boolean) {
    this.user.Printers.forEach((printer) => printer.IsAssigned = status);
  }
}
