import { Component, input, output } from '@angular/core';
import { IconButton } from '../../icon-button/icon-button';
import { AriaLabel, IconName } from '../../../../app.enum';
import { fadeIn } from '../../../../utils/animations';
import { FADE_IN_ANIMATION_DURATION_MS } from '../../../../app.constants';
import { ParentModalLayout } from '../../../../core/directives/parent-modal-layout';

@Component({
  selector: 'app-delete-confirmation-modal',
  imports: [IconButton],
  templateUrl: './delete-confirmation-modal.html',
  styleUrl: './delete-confirmation-modal.scss',
  animations: [fadeIn(FADE_IN_ANIMATION_DURATION_MS)],
})
export class DeleteConfirmationModal extends ParentModalLayout  {
  readonly participantName = input.required<string>();
  
  readonly confirmDelete = output<void>();

  public readonly iconClose = IconName.Close;
  public readonly ariaLabelClose = AriaLabel.Close;

  public onConfirm(): void {
    this.confirmDelete.emit();
  }

  public onCancel(): void {
    this.closeModal.emit();
  }
}
