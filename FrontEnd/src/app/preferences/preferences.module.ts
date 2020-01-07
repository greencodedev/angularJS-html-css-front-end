import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PrinterSetupComponent } from './printer-setup/printer-setup.component';
import { PrintLogComponent } from './print-log/print-log.component';
import { UsersComponent } from './users/users.component';
import { PreferencesComponent } from './preferences.component';
import { PreferencesNavigationComponent } from './preferences-navigation/preferences-navigation.component';
import { RouterModule } from '@angular/router';
import { AccordionModule, BsDropdownModule, PaginationModule, TooltipModule, TypeaheadModule } from 'ngx-bootstrap';
import { FormsModule } from '@angular/forms';
import { PrinterSettingsComponent } from './printer-setup/printer-settings/printer-settings.component';
import { PrinterSetupModalComponent } from './printer-setup/printer-setup-modal/printer-setup-modal.component';
import { AddUserModalComponent } from './users/add-user-modal/add-user-modal.component';
import { MainPreferencesNavigationComponent } from './main-preferences-navigation/main-preferences-navigation.component';
import { LocationsComponent } from './products/locations/locations.component';
import { ConditionsComponent } from './products/conditions/conditions.component';
import { ConvertLogComponent } from './convert-log/convert-log.component';
import { ConditionModalComponent } from './products/conditions/condition-modal/condition-modal.component';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { AvgCostLogComponent } from './products/customCost/avg-cost-log/avg-cost-log.component';

@NgModule({
  declarations: [
    PrinterSetupComponent,
    PrintLogComponent,
    UsersComponent,
    PreferencesComponent,
    PreferencesNavigationComponent,
    PrinterSettingsComponent,
    PrinterSetupModalComponent,
    AddUserModalComponent,
    MainPreferencesNavigationComponent,
    LocationsComponent,
    ConditionsComponent,
    ConvertLogComponent,
    ConditionModalComponent,
    AvgCostLogComponent
  ],
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    BsDropdownModule.forRoot(),
    PaginationModule.forRoot(),
    AccordionModule.forRoot(),
    TooltipModule.forRoot(),
    NgxDaterangepickerMd.forRoot(),
    TypeaheadModule.forRoot(),
  ],
  entryComponents: [PrinterSetupModalComponent, AddUserModalComponent, ConditionModalComponent]
})
export class PreferencesModule {
}
